using System.Text.Json;
using GestionLudopatas.Api.Application.Pendientes;

namespace GestionLudopatas.Api.Endpoints;

/// <summary>Los 6 endpoints de búsqueda de pendientes (specs pendientes-*).</summary>
public static class PendientesEndpoints
{
    public static IEndpointRouteBuilder MapPendientesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/pendientes/calimaco/ingresos/busqueda", (JsonElement cuerpo, ManejadorBuscarPendientesCalimacoIngreso manejador, HttpContext http, CancellationToken ct) =>
                BuscarAsync(manejador, cuerpo, http, ct))
            .WithName("buscarPendientesCalimacoIngreso");

        app.MapPost("/api/v1/pendientes/calimaco/salidas/busqueda", (JsonElement cuerpo, ManejadorBuscarPendientesCalimacoSalida manejador, HttpContext http, CancellationToken ct) =>
                BuscarAsync(manejador, cuerpo, http, ct))
            .WithName("buscarPendientesCalimacoSalida");

        app.MapPost("/api/v1/pendientes/cmp/ingresos/busqueda", (JsonElement cuerpo, ManejadorBuscarPendientesCmpIngreso manejador, HttpContext http, CancellationToken ct) =>
                BuscarAsync(manejador, cuerpo, http, ct))
            .WithName("buscarPendientesCmpIngreso");

        app.MapPost("/api/v1/pendientes/cmp/salidas/busqueda", (JsonElement cuerpo, ManejadorBuscarPendientesCmpSalida manejador, HttpContext http, CancellationToken ct) =>
                BuscarAsync(manejador, cuerpo, http, ct))
            .WithName("buscarPendientesCmpSalida");

        app.MapPost("/api/v1/pendientes/sica/ingresos/busqueda", async (PendientesSicaRequest cuerpo, ManejadorBuscarPendientesSicaIngreso manejador, HttpContext http, CancellationToken ct) =>
                ResultadoHttp.Responder(await manejador.EjecutarAsync(cuerpo, ct), http, items => Results.Json(items)))
            .WithName("buscarPendientesSicaIngreso");

        app.MapPost("/api/v1/pendientes/sica/salidas/busqueda", async (PendientesSicaRequest cuerpo, ManejadorBuscarPendientesSicaSalida manejador, HttpContext http, CancellationToken ct) =>
                ResultadoHttp.Responder(await manejador.EjecutarAsync(cuerpo, ct), http, items => Results.Json(items)))
            .WithName("buscarPendientesSicaSalida");

        return app;
    }

    /// <summary>
    /// CALIMACO/CMP se parsean desde <see cref="JsonElement"/> (no binding directo a un
    /// record) porque el contrato distingue <c>esReintentoForzado</c> omitido de enviado-null
    /// — ver <see cref="PendientesCalimacoCmpRequestJson"/>.
    /// </summary>
    private static async Task<IResult> BuscarAsync<TItem, TPuerto>(
        ManejadorBuscarPendientesCalimacoCmpBase<TItem, TPuerto> manejador, JsonElement cuerpo, HttpContext http, CancellationToken ct)
        where TPuerto : IBuscarPendientes<PendientesCalimacoCmpRequest, TItem>
    {
        var request = PendientesCalimacoCmpRequestJson.DesdeJson(cuerpo);
        if (!request.IsSuccess)
            return ResultadoHttp.ResponderError(request.Error, http);

        var resultado = await manejador.EjecutarAsync(request.Value, ct);
        return ResultadoHttp.Responder(resultado, http, items => Results.Json(items));
    }
}
