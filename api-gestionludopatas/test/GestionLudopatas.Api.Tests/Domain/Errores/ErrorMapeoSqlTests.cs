using GestionLudopatas.Api.Domain.Errores;
using GestionLudopatas.Api.Errores;
using Xunit;

namespace GestionLudopatas.Api.Tests.Domain.Errores;

/// <summary>Tarea 9.1 — cada uno de los 23 números runtime resuelve al code/status/retryable exacto de la matriz.</summary>
public class ErrorMapeoSqlTests
{
    [Theory]
    [InlineData(51000, 422, CodigoError.CorteResolverFechaEvaluacionRequerida, false)]
    [InlineData(51001, 422, CodigoError.CorteResolverTimeoutInvalido, false)]
    [InlineData(51002, 409, CodigoError.CorteResolverConflictoOficiales, false)]
    [InlineData(51003, 409, CodigoError.CorteResolverConflictoManuales, false)]
    [InlineData(51004, 422, CodigoError.CorteResolverTimeoutFueraDeRango, false)]
    [InlineData(51010, 422, CodigoError.CorteCrearTipoInvalido, false)]
    [InlineData(51011, 422, CodigoError.CorteCrearFechaCorteRequerida, false)]
    [InlineData(51012, 422, CodigoError.CorteCrearFechaCorteDebeSerNula, false)]
    [InlineData(51013, 422, CodigoError.CorteCrearFechaEjecucionRequerida, false)]
    [InlineData(51100, 422, CodigoError.PendCalimacoIngresoCorteInvalido, false)]
    [InlineData(51101, 422, CodigoError.PendCalimacoIngresoMaxReintentosInvalido, false)]
    [InlineData(51102, 422, CodigoError.PendCalimacoIngresoReintentoForzadoInvalido, false)]
    [InlineData(51110, 422, CodigoError.PendCalimacoSalidaCorteInvalido, false)]
    [InlineData(51111, 422, CodigoError.PendCalimacoSalidaMaxReintentosInvalido, false)]
    [InlineData(51112, 422, CodigoError.PendCalimacoSalidaReintentoForzadoInvalido, false)]
    [InlineData(51120, 422, CodigoError.PendCmpIngresoCorteInvalido, false)]
    [InlineData(51121, 422, CodigoError.PendCmpIngresoMaxReintentosInvalido, false)]
    [InlineData(51122, 422, CodigoError.PendCmpIngresoReintentoForzadoInvalido, false)]
    [InlineData(51130, 422, CodigoError.PendCmpSalidaCorteInvalido, false)]
    [InlineData(51131, 422, CodigoError.PendCmpSalidaMaxReintentosInvalido, false)]
    [InlineData(51132, 422, CodigoError.PendCmpSalidaReintentoForzadoInvalido, false)]
    [InlineData(51140, 422, CodigoError.PendSicaIngresoMaxReintentosInvalido, false)]
    [InlineData(51150, 422, CodigoError.PendSicaSalidaMaxReintentosInvalido, false)]
    public void Resolver_para_cada_numero_runtime_devuelve_status_code_y_retryable_de_la_matriz(
        int sqlErrorNumber, int statusEsperado, string codigoEsperado, bool retryableEsperado)
    {
        var mapeo = ErrorMapeoSql.Resolver(sqlErrorNumber);

        Assert.Equal(statusEsperado, mapeo.Status);
        Assert.Equal(codigoEsperado, mapeo.Codigo);
        Assert.Equal(retryableEsperado, mapeo.Reintentable);
    }

    [Theory]
    [InlineData(50000)]
    [InlineData(50001)]
    [InlineData(50002)]
    [InlineData(50010)]
    [InlineData(50011)]
    [InlineData(50012)]
    public void EsErrorDeDespliegue_reconoce_el_rango_50000_50012(int numero) =>
        Assert.True(ErrorMapeoSql.EsErrorDeDespliegue(numero));

    [Theory]
    [InlineData(51000)]
    [InlineData(2627)]
    [InlineData(1205)]
    public void EsErrorDeDespliegue_no_marca_como_despliegue_los_codigos_runtime(int numero) =>
        Assert.False(ErrorMapeoSql.EsErrorDeDespliegue(numero));

    [Theory]
    [InlineData(2627, 409, CodigoError.ConflictoDatosPrimaria, false)]
    [InlineData(2601, 409, CodigoError.ConflictoDatosUnicidad, false)]
    [InlineData(547, 409, CodigoError.ConflictoDatosReferencial, false)]
    [InlineData(-2, 504, CodigoError.SqlTimeout, true)]
    [InlineData(1222, 504, CodigoError.SqlTimeout, true)]
    [InlineData(1205, 503, CodigoError.SqlDeadlock, true)]
    [InlineData(4060, 503, CodigoError.SqlNoDisponible, true)]
    [InlineData(229, 503, CodigoError.SqlSinPermisos, false)]
    public void Resolver_para_codigos_nativos_sql_server(int sqlErrorNumber, int statusEsperado, string codigoEsperado, bool retryableEsperado)
    {
        var mapeo = ErrorMapeoSql.Resolver(sqlErrorNumber);

        Assert.Equal(statusEsperado, mapeo.Status);
        Assert.Equal(codigoEsperado, mapeo.Codigo);
        Assert.Equal(retryableEsperado, mapeo.Reintentable);
    }

    [Fact]
    public void Resolver_para_numero_no_catalogado_cae_al_fallback_no_clasificado()
    {
        var mapeo = ErrorMapeoSql.Resolver(999999);

        Assert.Equal(500, mapeo.Status);
        Assert.Equal(CodigoError.ApiNoPrevisto, mapeo.Codigo);
    }
}
