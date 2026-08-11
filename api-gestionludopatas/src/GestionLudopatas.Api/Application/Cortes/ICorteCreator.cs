namespace GestionLudopatas.Api.Application.Cortes;

public sealed record CrearCorteRequest(string? TipoCorte, DateTimeOffset? FechaHoraCorte, DateTimeOffset? FechaHoraEjecucion);

public sealed record CrearCorteResponse(int CorteId);

/// <summary>Puerto para <c>dbo.SP_CORTE_Crear</c> (spec corte-crear).</summary>
public interface ICorteCreator
{
    Task<CrearCorteResponse> CrearAsync(CrearCorteRequest request, CancellationToken ct);
}
