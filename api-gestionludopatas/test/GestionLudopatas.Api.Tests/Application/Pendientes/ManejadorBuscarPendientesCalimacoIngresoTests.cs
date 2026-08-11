using GestionLudopatas.Api.Application.Pendientes;
using GestionLudopatas.Api.Domain.Errores;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestionLudopatas.Api.Tests.Application.Pendientes;

public class ManejadorBuscarPendientesCalimacoIngresoTests
{
    private sealed class PuertoCanario : IBuscarPendientesCalimacoIngreso
    {
        public Task<IReadOnlyList<PendienteCalimacoItem>> BuscarAsync(PendientesCalimacoCmpRequest request, CancellationToken ct) =>
            throw new InvalidOperationException("No debía invocar el puerto — la validación debió cortar antes.");
    }

    private static readonly ManejadorBuscarPendientesCalimacoIngreso Manejador = new(new PuertoCanario());

    [Fact]
    public async Task CorteIdActual_invalido_devuelve_fallo_GL_PEND_CAL_ING_001()
    {
        var resultado = await Manejador.EjecutarAsync(new PendientesCalimacoCmpRequest(0, 3, true, true), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.PendCalimacoIngresoCorteInvalido, resultado.Error.Codigo);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, resultado.Error.Status);
    }

    [Fact]
    public async Task MaxReintentosPorSistema_invalido_devuelve_fallo_GL_PEND_CAL_ING_002()
    {
        var resultado = await Manejador.EjecutarAsync(new PendientesCalimacoCmpRequest(1, 0, true, true), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.PendCalimacoIngresoMaxReintentosInvalido, resultado.Error.Codigo);
    }

    [Fact]
    public async Task EsReintentoForzado_enviado_null_devuelve_fallo_GL_PEND_CAL_ING_003()
    {
        var resultado = await Manejador.EjecutarAsync(new PendientesCalimacoCmpRequest(1, 3, null, true), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.PendCalimacoIngresoReintentoForzadoInvalido, resultado.Error.Codigo);
    }

    [Fact]
    public async Task Solicitud_valida_invoca_puerto_y_devuelve_Ok()
    {
        var item = new PendienteCalimacoItem(1, "12345678", "DNI", 10, 9, null, 0);
        var manejador = new ManejadorBuscarPendientesCalimacoIngreso(new PuertoFalso(item));

        var resultado = await manejador.EjecutarAsync(new PendientesCalimacoCmpRequest(1, 3, false, true), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Single(resultado.Value);
    }

    private sealed class PuertoFalso(PendienteCalimacoItem item) : IBuscarPendientesCalimacoIngreso
    {
        public Task<IReadOnlyList<PendienteCalimacoItem>> BuscarAsync(PendientesCalimacoCmpRequest request, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<PendienteCalimacoItem>>([item]);
    }
}
