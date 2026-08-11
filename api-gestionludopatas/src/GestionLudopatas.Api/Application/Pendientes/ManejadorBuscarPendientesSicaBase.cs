using GestionLudopatas.Api.Application.Resultados;

namespace GestionLudopatas.Api.Application.Pendientes;

/// <summary>Base común a los 2 casos de uso de pendientes SICA (D4) — SICA solo valida <c>@MaxReintentosPorSistema</c>.</summary>
public abstract class ManejadorBuscarPendientesSicaBase<TItem>(IBuscarPendientes<PendientesSicaRequest, TItem> puerto)
{
    protected abstract string CodigoMaxReintentosInvalido { get; }

    public async Task<Result<IReadOnlyList<TItem>>> EjecutarAsync(PendientesSicaRequest request, CancellationToken ct)
    {
        if (request.MaxReintentosPorSistema is null or <= 0)
            return Result<IReadOnlyList<TItem>>.Fallo(StatusCodes.Status422UnprocessableEntity,
                CodigoMaxReintentosInvalido, "maxReintentosPorSistema debe ser mayor que cero.");

        var items = await puerto.BuscarAsync(request, ct);
        return Result<IReadOnlyList<TItem>>.Ok(items);
    }
}
