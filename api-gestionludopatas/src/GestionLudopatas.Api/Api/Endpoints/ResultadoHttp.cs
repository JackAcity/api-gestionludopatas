using GestionLudopatas.Api.Application.Resultados;
using GestionLudopatas.Api.Application.Errores;
using GestionLudopatas.Api.Errores;

namespace GestionLudopatas.Api.Endpoints;

/// <summary>
/// Traduce <see cref="Result{T}"/> a <see cref="IResult"/> — un solo lugar, no un
/// switch por endpoint (mismo criterio DRY que D3 del change original). Si el caso de
/// uso tuvo éxito, delega en <paramref name="exito"/> (para elegir 200/201/etc.); si
/// falló, arma el mismo <see cref="ProblemaDetalle"/> que antes producía
/// <c>ManejadorExcepcionesGlobal</c> para una <see cref="ErrorFuncionalException"/>
/// equivalente (spec casos-uso-result-negocio: contrato HTTP idéntico).
/// </summary>
public static class ResultadoHttp
{
    public static IResult Responder<T>(Result<T> resultado, HttpContext http, Func<T, IResult> exito) =>
        resultado.IsSuccess ? exito(resultado.Value) : ProblemaDe(resultado.Error, http);

    /// <summary>Variante para el camino de éxito que necesita await (ej. persistir idempotencia antes de responder).</summary>
    public static async Task<IResult> ResponderAsync<T>(Result<T> resultado, HttpContext http, Func<T, Task<IResult>> exito) =>
        resultado.IsSuccess ? await exito(resultado.Value) : ProblemaDe(resultado.Error, http);

    /// <summary>Traduce un fallo ya clasificado en el borde HTTP sin fabricar un resultado exitoso artificial.</summary>
    public static IResult ResponderError(ResultadoError error, HttpContext http) => ProblemaDe(error, http);

    private static IResult ProblemaDe(ResultadoError error, HttpContext http)
    {
        var problema = ProblemaDetalle.Crear(
            ProblemaDetalle.TituloParaStatus(error.Status), error.Status, error.Codigo, error.Detalle,
            http.TraceIdentifier, error.Reintentable, error.Origen, error.SqlErrorNumber);

        return Results.Json(problema, contentType: "application/problem+json", statusCode: problema.Status);
    }
}
