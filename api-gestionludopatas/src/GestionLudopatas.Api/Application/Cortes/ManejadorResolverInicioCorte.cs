using GestionLudopatas.Api.Application.Resultados;
using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Application.Cortes;

/// <summary>Caso de uso de <c>resolverInicioCorte</c> (spec corte-resolver-inicio) — valida y delega en <see cref="ICorteResolver"/>.</summary>
public sealed class ManejadorResolverInicioCorte(ICorteResolver resolver)
{
    public async Task<Result<ResolverInicioResponse>> EjecutarAsync(ResolverInicioRequest request, CancellationToken ct)
    {
        if (request.FechaHoraEvaluacion is null)
            return Result<ResolverInicioResponse>.Fallo(StatusCodes.Status422UnprocessableEntity,
                CodigoError.CorteResolverFechaEvaluacionRequerida, "fechaHoraEvaluacion es obligatoria.");

        if (request.TimeoutMinutos is null or < 0)
            return Result<ResolverInicioResponse>.Fallo(StatusCodes.Status422UnprocessableEntity,
                CodigoError.CorteResolverTimeoutInvalido, "timeoutMinutos debe ser mayor o igual que cero.");

        var respuesta = await resolver.ResolverAsync(request, ct);
        return Result<ResolverInicioResponse>.Ok(respuesta);
    }
}
