using GestionLudopatas.Api.Application.Resultados;
using GestionLudopatas.Api.Application.Errores;
using Xunit;

namespace GestionLudopatas.Api.Tests.Application.Resultados;

public class ResultTests
{
    [Fact]
    public void Ok_expone_Value_e_IsSuccess_true()
    {
        var resultado = Result<int>.Ok(42);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(42, resultado.Value);
    }

    [Fact]
    public void Ok_acceder_a_Error_lanza()
    {
        var resultado = Result<int>.Ok(42);

        Assert.Throws<InvalidOperationException>(() => resultado.Error);
    }

    [Fact]
    public void Fallo_expone_Error_e_IsSuccess_false()
    {
        var error = new ResultadoError(422, "GL-TEST-001", "detalle", false, "api");

        var resultado = Result<int>.Fallo(error);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(error, resultado.Error);
    }

    [Fact]
    public void Fallo_acceder_a_Value_lanza()
    {
        var resultado = Result<int>.Fallo(422, "GL-TEST-001", "detalle");

        Assert.Throws<InvalidOperationException>(() => resultado.Value);
    }

    [Fact]
    public void Fallo_con_parametros_sueltos_arma_el_mismo_ResultadoError()
    {
        var resultado = Result<int>.Fallo(409, "GL-TEST-002", "conflicto", reintentable: true, origen: "sql", sqlErrorNumber: 2627);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(new ResultadoError(409, "GL-TEST-002", "conflicto", true, "sql", 2627), resultado.Error);
    }
}
