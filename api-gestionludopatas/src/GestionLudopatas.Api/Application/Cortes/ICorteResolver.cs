namespace GestionLudopatas.Api.Application.Cortes;

public sealed record ResolverInicioRequest(DateTimeOffset? FechaHoraEvaluacion, int? TimeoutMinutos);

public sealed record ResolverInicioResponse(string Accion, int? CorteId, int? CorteColgadoOficialId, int? CorteColgadoManualId);

/// <summary>Puerto para <c>dbo.SP_CORTE_ResolverInicio</c> (spec corte-resolver-inicio).</summary>
public interface ICorteResolver
{
    Task<ResolverInicioResponse> ResolverAsync(ResolverInicioRequest request, CancellationToken ct);
}
