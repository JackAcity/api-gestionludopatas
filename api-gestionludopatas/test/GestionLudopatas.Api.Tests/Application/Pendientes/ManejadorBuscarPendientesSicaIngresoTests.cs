using GestionLudopatas.Api.Application.Pendientes;
using GestionLudopatas.Api.Domain.Errores;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestionLudopatas.Api.Tests.Application.Pendientes;

public class ManejadorBuscarPendientesSicaIngresoTests
{
    private sealed class PuertoCanario : IBuscarPendientes<PendientesSicaRequest, PendienteSicaIngresoItem>
    {
        public Task<IReadOnlyList<PendienteSicaIngresoItem>> BuscarAsync(PendientesSicaRequest request, CancellationToken ct) =>
            throw new InvalidOperationException("No debía invocar el puerto — la validación debió cortar antes.");
    }

    private static readonly ManejadorBuscarPendientesSicaIngreso Manejador = new(new PuertoCanario());

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task MaxReintentosPorSistema_invalido_devuelve_fallo_GL_PEND_SICA_ING_001(int? maxReintentos)
    {
        var resultado = await Manejador.EjecutarAsync(new PendientesSicaRequest(maxReintentos), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(CodigoError.PendSicaIngresoMaxReintentosInvalido, resultado.Error.Codigo);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, resultado.Error.Status);
    }
}
