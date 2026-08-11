using GestionLudopatas.Api.Domain.Errores;
using GestionLudopatas.Api.Errores;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestionLudopatas.Api.Tests.Application.Errores;

/// <summary>
/// Tarea 9.2 — precedencia de validación API↔SQL (spec modelo-error-comun, sección 5.1):
/// si la API prevalida una condición que el SP también valida, el código debe ser el mismo
/// que si el SP la hubiera detectado primero.
/// </summary>
public class PrecedenciaTests
{
    [Fact]
    public void Prevalidacion_api_y_error_sql_para_la_misma_condicion_devuelven_el_mismo_codigo()
    {
        var prevalidacionApi = ErrorFuncionalException.DeReglaEspecifica(
            StatusCodes.Status422UnprocessableEntity, CodigoError.CorteResolverFechaEvaluacionRequerida,
            "fechaHoraEvaluacion es obligatoria.");

        var mapeoSql = ErrorMapeoSql.Resolver(51000); // mismo THROW SQL: @FechaHoraEvaluacion IS NULL
        var errorSql = ErrorFuncionalException.DeSql(mapeoSql, 51000);

        Assert.Equal(prevalidacionApi.Codigo, errorSql.Codigo);
        Assert.Equal(prevalidacionApi.Status, errorSql.Status);
    }

    [Fact]
    public void Origen_distingue_api_de_sql_aunque_el_codigo_sea_el_mismo()
    {
        var prevalidacionApi = ErrorFuncionalException.DeReglaEspecifica(422, CodigoError.CorteCrearTipoInvalido, "detalle");
        var errorSql = ErrorFuncionalException.DeSql(ErrorMapeoSql.Resolver(51010), 51010);

        Assert.Equal("api", prevalidacionApi.Origen);
        Assert.Equal("sql", errorSql.Origen);
        Assert.Equal(prevalidacionApi.Codigo, errorSql.Codigo);
    }

    [Fact]
    public void Contrato_generico_usa_el_fallback_GL_API_REQ_002()
    {
        var error = ErrorFuncionalException.DeContratoGenerico("detalle cualquiera");

        Assert.Equal(CodigoError.ApiContratoInvalido, error.Codigo);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, error.Status);
        Assert.Equal("api", error.Origen);
    }
}
