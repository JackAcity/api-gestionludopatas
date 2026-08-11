using GestionLudopatas.Api.Application.Resultados;
using GestionLudopatas.Api.Application.Errores;
using GestionLudopatas.Api.Errores;
using GestionLudopatas.Api.Endpoints;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestionLudopatas.Api.Tests.Endpoints;

public class ResultadoHttpTests
{
    [Fact]
    public void Fallo_de_negocio_devuelve_problem_json()
    {
        var contexto = new DefaultHttpContext();
        var resultado = Result<int>.Fallo(
            StatusCodes.Status422UnprocessableEntity, "GL-TEST-001", "detalle de prueba");

        var respuesta = ResultadoHttp.Responder(resultado, contexto, valor => Results.Ok(valor));

        var json = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<ProblemaDetalle>>(respuesta);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, json.StatusCode);
        Assert.Equal("application/problem+json", json.ContentType);
    }

    [Fact]
    public void Error_de_parseo_clasificado_devuelve_problem_json_400()
    {
        var contexto = new DefaultHttpContext();
        var error = new ResultadoError(
            StatusCodes.Status400BadRequest, "GL-API-REQ-001", "JSON inválido.", false, "api");

        var respuesta = ResultadoHttp.ResponderError(error, contexto);

        var json = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<ProblemaDetalle>>(respuesta);
        Assert.Equal(StatusCodes.Status400BadRequest, json.StatusCode);
        Assert.Equal("GL-API-REQ-001", json.Value!.Code);
        Assert.Equal("application/problem+json", json.ContentType);
    }
}
