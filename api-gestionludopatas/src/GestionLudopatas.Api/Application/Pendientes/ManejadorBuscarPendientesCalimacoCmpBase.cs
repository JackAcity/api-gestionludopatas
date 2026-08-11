using GestionLudopatas.Api.Application.Resultados;
using GestionLudopatas.Api.Application.Errores;

namespace GestionLudopatas.Api.Application.Pendientes;

/// <summary>
/// Base común a los 4 casos de uso de pendientes CALIMACO/CMP (D4) — mismo criterio que
/// <c>CalimacoCmpBuscadorSqlBase</c> en Infrastructure: valida y delega en el puerto;
/// las 4 subclases finas solo proveen sus códigos <c>GL-*</c>.
/// </summary>
public abstract class ManejadorBuscarPendientesCalimacoCmpBase<TItem, TPuerto>(TPuerto puerto)
    where TPuerto : IBuscarPendientes<PendientesCalimacoCmpRequest, TItem>
{
    protected abstract string CodigoCorteInvalido { get; }
    protected abstract string CodigoMaxReintentosInvalido { get; }
    protected abstract string CodigoReintentoForzadoInvalido { get; }

    public async Task<Result<IReadOnlyList<TItem>>> EjecutarAsync(PendientesCalimacoCmpRequest request, CancellationToken ct)
    {
        var fallo = Validar(request);
        if (fallo is not null)
            return Result<IReadOnlyList<TItem>>.Fallo(fallo);

        var items = await puerto.BuscarAsync(request, ct);
        return Result<IReadOnlyList<TItem>>.Ok(items);
    }

    private ResultadoError? Validar(PendientesCalimacoCmpRequest request)
    {
        if (request.CorteIdActual is null or <= 0)
            return new ResultadoError(StatusCodes.Status422UnprocessableEntity, CodigoCorteInvalido, "corteIdActual debe ser mayor que cero.", false, "api");

        if (request.MaxReintentosPorSistema is null or <= 0)
            return new ResultadoError(StatusCodes.Status422UnprocessableEntity, CodigoMaxReintentosInvalido, "maxReintentosPorSistema debe ser mayor que cero.", false, "api");

        if (request is { EsReintentoForzadoEnviado: true, EsReintentoForzado: null })
            return new ResultadoError(StatusCodes.Status422UnprocessableEntity, CodigoReintentoForzadoInvalido, "esReintentoForzado no puede ser null.", false, "api");

        return null;
    }
}
