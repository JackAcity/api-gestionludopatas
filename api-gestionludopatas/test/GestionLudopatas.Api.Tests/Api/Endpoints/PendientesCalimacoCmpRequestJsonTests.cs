using System.Text.Json;
using GestionLudopatas.Api.Endpoints;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestionLudopatas.Api.Tests.Endpoints;

public class PendientesCalimacoCmpRequestJsonTests
{
    [Theory]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("true")]
    public void Raiz_que_no_es_objeto_devuelve_400_GL_API_REQ_001(string cuerpo)
    {
        var resultado = PendientesCalimacoCmpRequestJson.DesdeJson(Elemento(cuerpo));

        Assert.False(resultado.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, resultado.Error.Status);
        Assert.Equal("GL-API-REQ-001", resultado.Error.Codigo);
    }

    [Theory]
    [InlineData("{\"corteIdActual\":1.5,\"maxReintentosPorSistema\":1}")]
    [InlineData("{\"corteIdActual\":2147483648,\"maxReintentosPorSistema\":1}")]
    [InlineData("{\"corteIdActual\":\"1\",\"maxReintentosPorSistema\":1}")]
    [InlineData("{\"corteIdActual\":1,\"maxReintentosPorSistema\":false}")]
    public void Entero_decimal_fuera_de_rango_o_de_otro_tipo_devuelve_400_GL_API_REQ_001(string cuerpo)
    {
        var resultado = PendientesCalimacoCmpRequestJson.DesdeJson(Elemento(cuerpo));

        Assert.False(resultado.IsSuccess);
        Assert.Equal("GL-API-REQ-001", resultado.Error.Codigo);
    }

    [Fact]
    public void Booleano_de_otro_tipo_devuelve_400_GL_API_REQ_001()
    {
        var resultado = PendientesCalimacoCmpRequestJson.DesdeJson(
            Elemento("{\"corteIdActual\":1,\"maxReintentosPorSistema\":1,\"esReintentoForzado\":\"false\"}"));

        Assert.False(resultado.IsSuccess);
        Assert.Equal(StatusCodes.Status400BadRequest, resultado.Error.Status);
        Assert.Equal("GL-API-REQ-001", resultado.Error.Codigo);
    }

    [Fact]
    public void Booleano_nulo_explicito_se_conserva_para_la_regla_funcional_422()
    {
        var resultado = PendientesCalimacoCmpRequestJson.DesdeJson(
            Elemento("{\"corteIdActual\":1,\"maxReintentosPorSistema\":1,\"esReintentoForzado\":null}"));

        Assert.True(resultado.IsSuccess);
        Assert.Null(resultado.Value.EsReintentoForzado);
        Assert.True(resultado.Value.EsReintentoForzadoEnviado);
    }

    private static JsonElement Elemento(string json) => JsonDocument.Parse(json).RootElement.Clone();
}
