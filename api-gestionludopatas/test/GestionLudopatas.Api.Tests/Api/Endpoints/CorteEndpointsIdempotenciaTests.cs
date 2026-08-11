using GestionLudopatas.Api.Application.Cortes;
using GestionLudopatas.Api.Errores;
using GestionLudopatas.Api.Domain.Errores;
using GestionLudopatas.Api.Endpoints;
using GestionLudopatas.Api.Infrastructure.Idempotencia;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestionLudopatas.Api.Tests.Endpoints;

/// <summary>
/// Tarea 9.3 — idempotencia de crearCorte (D7) con <see cref="ICorteCreator"/> fake y
/// <see cref="IdempotencyStore"/> real en memoria. No toca bd_autobot.
/// </summary>
public class CorteEndpointsIdempotenciaTests
{
    private sealed class CorteCreatorFake : ICorteCreator
    {
        public int VecesLlamado { get; private set; }

        public Task<CrearCorteResponse> CrearAsync(CrearCorteRequest request, CancellationToken ct)
        {
            VecesLlamado++;
            return Task.FromResult(new CrearCorteResponse(CorteId: 126));
        }
    }

    private sealed class CorteCreatorBloqueante : ICorteCreator
    {
        private readonly TaskCompletionSource _continuar = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int VecesLlamado { get; private set; }
        public TaskCompletionSource Inicio { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<CrearCorteResponse> CrearAsync(CrearCorteRequest request, CancellationToken ct)
        {
            VecesLlamado++;
            Inicio.TrySetResult();
            await _continuar.Task.WaitAsync(ct);
            return new CrearCorteResponse(CorteId: 126);
        }

        public void Continuar() => _continuar.TrySetResult();
    }

    private static HttpContext ContextoConHeader(string valor)
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.Headers[CorteEndpoints.EncabezadoIdempotencyKey] = valor;
        return contexto;
    }

    private static readonly CrearCorteRequest Payload = new("manual", null, DateTimeOffset.Parse("2026-08-05T08:12:00-05:00"));
    private const string Clave = "clave-idempotencia-de-16-mas"; // 16-128 caracteres

    [Fact]
    public async Task Primera_solicitud_ejecuta_el_puerto_y_devuelve_201()
    {
        var creador = new CorteCreatorFake();
        var manejador = new ManejadorCrearCorte(creador);
        var idempotencia = new IdempotencyStore();

        var resultado = await CorteEndpoints.CrearCorteAsync(Payload, ContextoConHeader(Clave), manejador, idempotencia, CancellationToken.None);

        Assert.Equal(1, creador.VecesLlamado);
        var creado = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Created<CrearCorteResponse>>(resultado);
        Assert.Equal(126, creado.Value!.CorteId);
    }

    [Fact]
    public async Task Misma_clave_mismo_payload_reproduce_la_respuesta_sin_ejecutar_el_puerto_de_nuevo()
    {
        var creador = new CorteCreatorFake();
        var manejador = new ManejadorCrearCorte(creador);
        var idempotencia = new IdempotencyStore();

        await CorteEndpoints.CrearCorteAsync(Payload, ContextoConHeader(Clave), manejador, idempotencia, CancellationToken.None);
        var contextoReplay = ContextoConHeader(Clave);
        var segundaRespuesta = await CorteEndpoints.CrearCorteAsync(Payload, contextoReplay, manejador, idempotencia, CancellationToken.None);

        Assert.Equal(1, creador.VecesLlamado); // no se volvió a ejecutar
        Assert.Equal("true", contextoReplay.Response.Headers["Idempotency-Replayed"]);
        var json = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<CrearCorteResponse>>(segundaRespuesta);
        Assert.Equal(126, json.Value!.CorteId);
    }

    [Fact]
    public async Task Solicitudes_simultaneas_con_misma_clave_ejecutan_el_puerto_una_sola_vez()
    {
        var creador = new CorteCreatorBloqueante();
        var manejador = new ManejadorCrearCorte(creador);
        var idempotencia = new IdempotencyStore();

        var primera = CorteEndpoints.CrearCorteAsync(Payload, ContextoConHeader(Clave), manejador, idempotencia, CancellationToken.None);
        await creador.Inicio.Task;
        var contextoRepetido = ContextoConHeader(Clave);
        var segunda = CorteEndpoints.CrearCorteAsync(Payload, contextoRepetido, manejador, idempotencia, CancellationToken.None);

        Assert.Equal(1, creador.VecesLlamado);
        creador.Continuar();
        var respuestas = await Task.WhenAll(primera, segunda);

        Assert.Equal(1, creador.VecesLlamado);
        Assert.Contains(respuestas, respuesta => respuesta is Microsoft.AspNetCore.Http.HttpResults.Created<CrearCorteResponse>);
        Assert.Equal("true", contextoRepetido.Response.Headers["Idempotency-Replayed"]);
    }

    [Fact]
    public async Task Solicitud_simultanea_con_misma_clave_y_payload_distinto_devuelve_409_sin_esperar_el_sp()
    {
        var creador = new CorteCreatorBloqueante();
        var manejador = new ManejadorCrearCorte(creador);
        var idempotencia = new IdempotencyStore();
        var payloadDistinto = Payload with { FechaHoraEjecucion = DateTimeOffset.Parse("2026-08-06T08:12:00-05:00") };

        var primera = CorteEndpoints.CrearCorteAsync(Payload, ContextoConHeader(Clave), manejador, idempotencia, CancellationToken.None);
        await creador.Inicio.Task;

        var excepcion = await Assert.ThrowsAsync<ErrorFuncionalException>(() =>
            CorteEndpoints.CrearCorteAsync(payloadDistinto, ContextoConHeader(Clave), manejador, idempotencia, CancellationToken.None));

        Assert.Equal(CodigoError.IdempotenciaConflicto, excepcion.Codigo);
        Assert.Equal(1, creador.VecesLlamado);
        creador.Continuar();
        await primera;
    }

    [Fact]
    public async Task Misma_clave_payload_distinto_devuelve_409_GL_IDEMP_001()
    {
        var creador = new CorteCreatorFake();
        var manejador = new ManejadorCrearCorte(creador);
        var idempotencia = new IdempotencyStore();

        await CorteEndpoints.CrearCorteAsync(Payload, ContextoConHeader(Clave), manejador, idempotencia, CancellationToken.None);

        var payloadDistinto = Payload with { FechaHoraEjecucion = DateTimeOffset.Parse("2026-08-06T08:12:00-05:00") };

        var excepcion = await Assert.ThrowsAsync<ErrorFuncionalException>(() =>
            CorteEndpoints.CrearCorteAsync(payloadDistinto, ContextoConHeader(Clave), manejador, idempotencia, CancellationToken.None));

        Assert.Equal(CodigoError.IdempotenciaConflicto, excepcion.Codigo);
        Assert.Equal(StatusCodes.Status409Conflict, excepcion.Status);
        Assert.Equal(1, creador.VecesLlamado); // el conflicto no ejecuta el puerto
    }

    [Theory]
    [InlineData("")]
    [InlineData("muy-corta")]
    public async Task Idempotency_key_fuera_de_rango_devuelve_422_GL_API_REQ_002(string claveInvalida)
    {
        var creador = new CorteCreatorFake();
        var manejador = new ManejadorCrearCorte(creador);
        var idempotencia = new IdempotencyStore();

        var excepcion = await Assert.ThrowsAsync<ErrorFuncionalException>(() =>
            CorteEndpoints.CrearCorteAsync(Payload, ContextoConHeader(claveInvalida), manejador, idempotencia, CancellationToken.None));

        Assert.Equal(CodigoError.ApiContratoInvalido, excepcion.Codigo);
        Assert.Equal(0, creador.VecesLlamado);
    }
}
