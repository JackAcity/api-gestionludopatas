using GestionLudopatas.Api.Domain.Errores;
using GestionLudopatas.Api.Errores;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace GestionLudopatas.Api.Middleware;

/// <summary>
/// Único punto que traduce excepciones a <see cref="ProblemaDetalle"/>. Nunca filtra
/// <c>ex.Message</c> crudo de una excepción no clasificada (500) — spec modelo-error-comun.
/// Los códigos 50000-50012 (despliegue/esquema) nunca se sirven tal cual como respuesta
/// runtime (D6): si un <see cref="SqlException"/> cae en ese rango durante una solicitud
/// de negocio (no debería — el healthcheck ya debió bloquear el arranque), se loguea como
/// crítico y se responde el fallback genérico no clasificado, no el código de despliegue.
/// </summary>
public sealed class ManejadorExcepcionesGlobal(ILogger<ManejadorExcepcionesGlobal> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problema = exception switch
        {
            BadHttpRequestException => SolicitudJsonInvalida(httpContext.TraceIdentifier),
            JsonException => SolicitudJsonInvalida(httpContext.TraceIdentifier),
            ErrorFuncionalException ex => ProblemaDetalle.Crear(
                ProblemaDetalle.TituloParaStatus(ex.Status), ex.Status, ex.Codigo, ex.Detalle,
                httpContext.TraceIdentifier, ex.Reintentable, ex.Origen, ex.SqlErrorNumber),

            SqlException ex => ManejarSqlException(ex, httpContext.TraceIdentifier),

            _ => ManejarNoClasificado(exception, httpContext.TraceIdentifier),
        };

        httpContext.Response.Headers[TrazabilidadMiddleware.EncabezadoTraceId] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = problema.Status;
        await httpContext.Response.WriteAsJsonAsync(
            problema,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
        return true;
    }

    private static ProblemaDetalle SolicitudJsonInvalida(string traceId) =>
        ProblemaDetalle.Crear(
            ProblemaDetalle.TituloParaStatus(StatusCodes.Status400BadRequest),
            StatusCodes.Status400BadRequest,
            CodigoError.ApiRequestInvalido,
            "El cuerpo JSON es inválido o contiene valores de tipos incompatibles.",
            traceId,
            retryable: false,
            source: "api");

    private ProblemaDetalle ManejarSqlException(SqlException ex, string traceId)
    {
        if (ErrorMapeoSql.EsErrorDeDespliegue(ex.Number))
        {
            logger.LogCritical(ex, "Error de despliegue/esquema {SqlErrorNumber} apareció en runtime — debió bloquear el healthcheck", ex.Number);
            return ManejarNoClasificado(ex, traceId);
        }

        // sqlErrorNumber solo viaja para números catalogados (spec modelo-error-comun: "puede
        // omitirse en fallos técnicos sensibles") — un número SQL nativo no catalogado (ej.
        // 18456 login failed) cae al fallback no clasificado, sin exponer el número.
        if (ErrorMapeoSql.PorNumero.TryGetValue(ex.Number, out var mapeo))
            return ProblemaDetalle.Crear(ProblemaDetalle.TituloParaStatus(mapeo.Status), mapeo.Status, mapeo.Codigo, mapeo.MensajePublico, traceId, mapeo.Reintentable, mapeo.Origen, ex.Number);

        logger.LogError(ex, "SqlException con número no catalogado {SqlErrorNumber}", ex.Number);
        return ManejarNoClasificado(ex, traceId);
    }

    private ProblemaDetalle ManejarNoClasificado(Exception ex, string traceId)
    {
        logger.LogError(ex, "Error no clasificado");
        return ProblemaDetalle.Crear("Error interno", StatusCodes.Status500InternalServerError, CodigoError.ApiNoPrevisto,
            "Ocurrió un error interno no previsto.", traceId, retryable: false, source: "api");
    }
}
