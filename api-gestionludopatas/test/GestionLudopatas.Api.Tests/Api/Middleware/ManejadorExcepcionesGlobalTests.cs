using System.Text.Json;
using GestionLudopatas.Api.Domain.Errores;
using GestionLudopatas.Api.Errores;
using GestionLudopatas.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GestionLudopatas.Api.Tests.Middleware;

public class ManejadorExcepcionesGlobalTests
{
    [Fact]
    public async Task Json_invalido_se_traduce_a_problem_json_400_GL_API_REQ_001()
    {
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();
        var manejador = new ManejadorExcepcionesGlobal(NullLogger<ManejadorExcepcionesGlobal>.Instance);

        var manejado = await manejador.TryHandleAsync(contexto, new JsonException("No se expone este mensaje."), CancellationToken.None);

        Assert.True(manejado);
        Assert.Equal(StatusCodes.Status400BadRequest, contexto.Response.StatusCode);
        Assert.Equal("application/problem+json", contexto.Response.ContentType);
        contexto.Response.Body.Position = 0;
        var problema = await JsonSerializer.DeserializeAsync<ProblemaDetalle>(contexto.Response.Body);
        Assert.NotNull(problema);
        Assert.Equal(CodigoError.ApiRequestInvalido, problema.Code);
        Assert.Equal("api", problema.Source);
    }
}
