using GestionLudopatas.Api.Application.Cortes;
using GestionLudopatas.Api.Domain.Errores;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestionLudopatas.Api.Tests.Application.Cortes;

public class ManejadorResolverInicioCorteTests
{
    private sealed class ResolverCanario : ICorteResolver
    {
        public Task<ResolverInicioResponse> ResolverAsync(ResolverInicioRequest request, CancellationToken ct) =>
            throw new InvalidOperationException("No debía invocar ICorteResolver — la validación debió cortar antes.");
    }

    private static readonly ManejadorResolverInicioCorte Manejador = new(new ResolverCanario());

    [Fact]
    public async Task FechaHoraEvaluacion_ausente_devuelve_fallo_GL_CORTE_RES_001_sin_invocar_resolver()
    {
        var resultado = await Manejador.EjecutarAsync(new ResolverInicioRequest(null, 30), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.CorteResolverFechaEvaluacionRequerida, resultado.Error.Codigo);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, resultado.Error.Status);
    }

    [Fact]
    public async Task TimeoutMinutos_negativo_devuelve_fallo_GL_CORTE_RES_002_sin_invocar_resolver()
    {
        var resultado = await Manejador.EjecutarAsync(new ResolverInicioRequest(DateTimeOffset.UtcNow, -1), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.CorteResolverTimeoutInvalido, resultado.Error.Codigo);
    }

    [Fact]
    public async Task Solicitud_valida_invoca_resolver_y_devuelve_Ok()
    {
        var esperada = new ResolverInicioResponse("crear_oficial", null, null, null);
        var manejador = new ManejadorResolverInicioCorte(new ResolverFalso(esperada));

        var resultado = await manejador.EjecutarAsync(new ResolverInicioRequest(DateTimeOffset.UtcNow, 30), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(esperada, resultado.Value);
    }

    private sealed class ResolverFalso(ResolverInicioResponse respuesta) : ICorteResolver
    {
        public Task<ResolverInicioResponse> ResolverAsync(ResolverInicioRequest request, CancellationToken ct) =>
            Task.FromResult(respuesta);
    }
}
