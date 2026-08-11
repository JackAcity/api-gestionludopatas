using GestionLudopatas.Api.Application.Resultados;
using GestionLudopatas.Api.Application.Errores;
using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Application.Cortes;

/// <summary>Caso de uso de <c>crearCorte</c> (spec corte-crear) — valida y delega en <see cref="ICorteCreator"/>.</summary>
public sealed class ManejadorCrearCorte(ICorteCreator creador)
{
    public async Task<Result<CrearCorteResponse>> EjecutarAsync(CrearCorteRequest request, CancellationToken ct)
    {
        var fallo = Validar(request);
        if (fallo is not null)
            return Result<CrearCorteResponse>.Fallo(fallo);

        var respuesta = await creador.CrearAsync(request, ct);
        return Result<CrearCorteResponse>.Ok(respuesta);
    }

    private static ResultadoError? Validar(CrearCorteRequest request)
    {
        if (request.TipoCorte is not ("oficial" or "manual"))
            return new ResultadoError(StatusCodes.Status422UnprocessableEntity,
                CodigoError.CorteCrearTipoInvalido, "tipoCorte solo permite oficial o manual.", false, "api");

        if (request.TipoCorte == "oficial" && request.FechaHoraCorte is null)
            return new ResultadoError(StatusCodes.Status422UnprocessableEntity,
                CodigoError.CorteCrearFechaCorteRequerida, "fechaHoraCorte es obligatoria para un corte oficial.", false, "api");

        if (request.TipoCorte == "manual" && request.FechaHoraCorte is not null)
            return new ResultadoError(StatusCodes.Status422UnprocessableEntity,
                CodigoError.CorteCrearFechaCorteDebeSerNula, "fechaHoraCorte debe omitirse o ser null para un corte manual.", false, "api");

        if (request.FechaHoraEjecucion is null)
            return new ResultadoError(StatusCodes.Status422UnprocessableEntity,
                CodigoError.CorteCrearFechaEjecucionRequerida, "fechaHoraEjecucion es obligatoria.", false, "api");

        return null;
    }
}
