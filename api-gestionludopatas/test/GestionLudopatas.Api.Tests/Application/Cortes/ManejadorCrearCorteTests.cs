using GestionLudopatas.Api.Application.Cortes;
using GestionLudopatas.Api.Domain.Errores;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestionLudopatas.Api.Tests.Application.Cortes;

/// <summary>
/// Reemplaza, para crearCorte, los casos que antes vivían en
/// ValidacionAdaptadoresTests.cs contra CorteCreatorSql directo (spec
/// casos-uso-result-negocio: la validación se movió al caso de uso).
/// </summary>
public class ManejadorCrearCorteTests
{
    private sealed class CreadorCanario : ICorteCreator
    {
        public Task<CrearCorteResponse> CrearAsync(CrearCorteRequest request, CancellationToken ct) =>
            throw new InvalidOperationException("No debía invocar ICorteCreator — la validación debió cortar antes.");
    }

    private static readonly ManejadorCrearCorte Manejador = new(new CreadorCanario());

    [Theory]
    [InlineData(null)]
    [InlineData("invalido")]
    public async Task TipoCorte_invalido_devuelve_fallo_GL_CORTE_CRE_001_sin_invocar_creador(string? tipo)
    {
        var resultado = await Manejador.EjecutarAsync(new CrearCorteRequest(tipo, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.CorteCrearTipoInvalido, resultado.Error.Codigo);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, resultado.Error.Status);
    }

    [Fact]
    public async Task Oficial_sin_fechaHoraCorte_devuelve_fallo_GL_CORTE_CRE_002()
    {
        var resultado = await Manejador.EjecutarAsync(new CrearCorteRequest("oficial", null, DateTimeOffset.UtcNow), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.CorteCrearFechaCorteRequerida, resultado.Error.Codigo);
    }

    [Fact]
    public async Task Manual_con_fechaHoraCorte_devuelve_fallo_GL_CORTE_CRE_003()
    {
        var resultado = await Manejador.EjecutarAsync(new CrearCorteRequest("manual", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.CorteCrearFechaCorteDebeSerNula, resultado.Error.Codigo);
    }

    [Fact]
    public async Task Sin_fechaHoraEjecucion_devuelve_fallo_GL_CORTE_CRE_004()
    {
        var resultado = await Manejador.EjecutarAsync(new CrearCorteRequest("manual", null, null), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.CorteCrearFechaEjecucionRequerida, resultado.Error.Codigo);
    }

    [Fact]
    public async Task Solicitud_valida_invoca_creador_y_devuelve_Ok()
    {
        var manejador = new ManejadorCrearCorte(new CreadorFalso());

        var resultado = await manejador.EjecutarAsync(new CrearCorteRequest("manual", null, DateTimeOffset.UtcNow), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(7, resultado.Value.CorteId);
    }

    private sealed class CreadorFalso : ICorteCreator
    {
        public Task<CrearCorteResponse> CrearAsync(CrearCorteRequest request, CancellationToken ct) =>
            Task.FromResult(new CrearCorteResponse(7));
    }
}
