using GestionLudopatas.Api.Application.Pendientes;
using GestionLudopatas.Api.Domain.Errores;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestionLudopatas.Api.Tests.Application.Pendientes;

public class ManejadorBuscarPendientesCalimacoSalidaTests
{
    private sealed class PuertoCanario : IBuscarPendientesCalimacoSalida
    {
        public Task<IReadOnlyList<PendienteCalimacoItem>> BuscarAsync(PendientesCalimacoCmpRequest request, CancellationToken ct) =>
            throw new InvalidOperationException("No debía invocar el puerto — la validación debió cortar antes.");
    }

    private static readonly ManejadorBuscarPendientesCalimacoSalida Manejador = new(new PuertoCanario());

    [Fact]
    public async Task CorteIdActual_invalido_devuelve_fallo_GL_PEND_CAL_SAL_001()
    {
        var resultado = await Manejador.EjecutarAsync(new PendientesCalimacoCmpRequest(null, 3, true, true), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.PendCalimacoSalidaCorteInvalido, resultado.Error.Codigo);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, resultado.Error.Status);
    }

    [Fact]
    public async Task MaxReintentosPorSistema_invalido_devuelve_fallo_GL_PEND_CAL_SAL_002()
    {
        var resultado = await Manejador.EjecutarAsync(new PendientesCalimacoCmpRequest(1, -1, true, true), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.PendCalimacoSalidaMaxReintentosInvalido, resultado.Error.Codigo);
    }

    [Fact]
    public async Task EsReintentoForzado_enviado_null_devuelve_fallo_GL_PEND_CAL_SAL_003()
    {
        var resultado = await Manejador.EjecutarAsync(new PendientesCalimacoCmpRequest(1, 3, null, true), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.PendCalimacoSalidaReintentoForzadoInvalido, resultado.Error.Codigo);
    }
}
