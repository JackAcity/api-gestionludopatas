using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GestionLudopatas.Api.Application.Cortes;
using GestionLudopatas.Api.Errores;
using GestionLudopatas.Api.Domain.Errores;
using GestionLudopatas.Api.Idempotencia;

namespace GestionLudopatas.Api.Endpoints;

/// <summary>Endpoints de cortes (specs corte-resolver-inicio, corte-crear).</summary>
public static class CorteEndpoints
{
    public const string EncabezadoIdempotencyKey = "Idempotency-Key";
    private const string EncabezadoIdempotencyReplayed = "Idempotency-Replayed";

    public static IEndpointRouteBuilder MapCorteEndpoints(this IEndpointRouteBuilder app)
    {
        // spec corte-resolver-inicio
        app.MapPost("/api/v1/cortes/resoluciones-inicio", async (
                ResolverInicioRequest cuerpo, ManejadorResolverInicioCorte manejador, HttpContext http, CancellationToken ct) =>
                ResultadoHttp.Responder(await manejador.EjecutarAsync(cuerpo, ct), http, respuesta => Results.Json(respuesta)))
            .WithName("resolverInicioCorte");

        // spec corte-crear — con idempotencia (D7)
        app.MapPost("/api/v1/cortes", CrearCorteAsync)
            .WithName("crearCorte");

        return app;
    }

    internal static async Task<IResult> CrearCorteAsync(
        CrearCorteRequest cuerpo, HttpContext http, ManejadorCrearCorte manejador, IIdempotencyStore idempotencia, CancellationToken ct)
    {
        var clave = http.Request.Headers[EncabezadoIdempotencyKey].ToString();
        if (clave.Length is < 16 or > 128)
            throw ErrorFuncionalException.DeContratoGenerico(
                $"El header {EncabezadoIdempotencyKey} es obligatorio y debe tener entre 16 y 128 caracteres.");

        var fingerprint = CalcularFingerprint(cuerpo);
        var reserva = await idempotencia.ReservarAsync(clave, fingerprint, ct);

        if (reserva is ConflictoReservaIdempotencia)
            throw ErrorFuncionalException.DeReglaEspecifica(StatusCodes.Status409Conflict,
                CodigoError.IdempotenciaConflicto, "La clave de idempotencia ya fue usada con otra solicitud.");

        if (reserva is ReproducirReservaIdempotencia reproducir)
        {
            http.Response.Headers[EncabezadoIdempotencyReplayed] = "true";
            return Results.Json(JsonSerializer.Deserialize<CrearCorteResponse>(reproducir.Registro.ResponseBodyJson), statusCode: reproducir.Registro.Status);
        }

        var ejecutar = (EjecutarReservaIdempotencia)reserva;
        try
        {
            var resultado = await manejador.EjecutarAsync(cuerpo, ct);
            if (!resultado.IsSuccess)
            {
                idempotencia.Cancelar(ejecutar.Reserva);
                return ResultadoHttp.Responder(resultado, http, respuesta => Results.Created($"/api/v1/cortes/{respuesta.CorteId}", respuesta));
            }

            var respuesta = resultado.Value;
            var cuerpoSerializado = JsonSerializer.Serialize(respuesta);
            idempotencia.Completar(ejecutar.Reserva, StatusCodes.Status201Created, cuerpoSerializado);
            return Results.Created($"/api/v1/cortes/{respuesta.CorteId}", respuesta);
        }
        catch
        {
            idempotencia.Cancelar(ejecutar.Reserva);
            throw;
        }
    }

    private static string CalcularFingerprint(CrearCorteRequest request)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)));
        return Convert.ToHexString(bytes);
    }
}
