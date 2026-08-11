namespace GestionLudopatas.Api.Domain.Errores;

/// <summary>
/// Códigos GL-* del contrato (endpoint/MATRIZ_MAPEO_ERRORES_SQL_HTTP.md). 29 THROW
/// reales (23 runtime + 6 deploy) más los transversales — no se inventan códigos
/// fuera de esta lista.
/// </summary>
public static class CodigoError
{
    // Transversales — sección 5 de la matriz
    public const string ApiRequestInvalido = "GL-API-REQ-001";
    public const string ApiContratoInvalido = "GL-API-REQ-002";
    public const string AutenticacionRequerida = "GL-AUTH-001";
    public const string AccesoDenegado = "GL-AUTH-002";
    public const string IdempotenciaConflicto = "GL-IDEMP-001";
    public const string ConflictoDatosPrimaria = "GL-DATA-CONFLICT-001";
    public const string ConflictoDatosUnicidad = "GL-DATA-CONFLICT-002";
    public const string ConflictoDatosReferencial = "GL-DATA-CONFLICT-003";
    public const string SqlTimeout = "GL-SQL-TIMEOUT-001";
    public const string SqlDeadlock = "GL-SQL-DEADLOCK-001";
    public const string SqlNoDisponible = "GL-SQL-UNAVAILABLE-001";
    public const string SqlSinPermisos = "GL-SQL-PERMISSION-001";
    public const string ApiSerializacion = "GL-API-SERIALIZATION-001";
    public const string ApiNoPrevisto = "GL-API-UNEXPECTED-001";

    // SP_CORTE_ResolverInicio — 51000-51004
    public const string CorteResolverFechaEvaluacionRequerida = "GL-CORTE-RES-001";
    public const string CorteResolverTimeoutInvalido = "GL-CORTE-RES-002";
    public const string CorteResolverConflictoOficiales = "GL-CORTE-RES-003";
    public const string CorteResolverConflictoManuales = "GL-CORTE-RES-004";
    public const string CorteResolverTimeoutFueraDeRango = "GL-CORTE-RES-005";

    // SP_CORTE_Crear — 51010-51013
    public const string CorteCrearTipoInvalido = "GL-CORTE-CRE-001";
    public const string CorteCrearFechaCorteRequerida = "GL-CORTE-CRE-002";
    public const string CorteCrearFechaCorteDebeSerNula = "GL-CORTE-CRE-003";
    public const string CorteCrearFechaEjecucionRequerida = "GL-CORTE-CRE-004";

    // SP_Pendientes_CALIMACO_Ingreso — 51100-51102
    public const string PendCalimacoIngresoCorteInvalido = "GL-PEND-CAL-ING-001";
    public const string PendCalimacoIngresoMaxReintentosInvalido = "GL-PEND-CAL-ING-002";
    public const string PendCalimacoIngresoReintentoForzadoInvalido = "GL-PEND-CAL-ING-003";

    // SP_Pendientes_CALIMACO_Salida — 51110-51112
    public const string PendCalimacoSalidaCorteInvalido = "GL-PEND-CAL-SAL-001";
    public const string PendCalimacoSalidaMaxReintentosInvalido = "GL-PEND-CAL-SAL-002";
    public const string PendCalimacoSalidaReintentoForzadoInvalido = "GL-PEND-CAL-SAL-003";

    // SP_Pendientes_CMP_Ingreso — 51120-51122
    public const string PendCmpIngresoCorteInvalido = "GL-PEND-CMP-ING-001";
    public const string PendCmpIngresoMaxReintentosInvalido = "GL-PEND-CMP-ING-002";
    public const string PendCmpIngresoReintentoForzadoInvalido = "GL-PEND-CMP-ING-003";

    // SP_Pendientes_CMP_Salida — 51130-51132
    public const string PendCmpSalidaCorteInvalido = "GL-PEND-CMP-SAL-001";
    public const string PendCmpSalidaMaxReintentosInvalido = "GL-PEND-CMP-SAL-002";
    public const string PendCmpSalidaReintentoForzadoInvalido = "GL-PEND-CMP-SAL-003";

    // SP_Pendientes_SICA_Ingreso — 51140
    public const string PendSicaIngresoMaxReintentosInvalido = "GL-PEND-SICA-ING-001";

    // SP_Pendientes_SICA_Salida — 51150
    public const string PendSicaSalidaMaxReintentosInvalido = "GL-PEND-SICA-SAL-001";

    // Despliegue/esquema — 50000-50012, nunca respuesta runtime (D6)
    public const string DeployContextoInvalido = "GL-SQL-DEPLOY-001";
    public const string DeploySchemaCorteIncompleto = "GL-SQL-SCHEMA-001";
    public const string DeploySchemaBitacoraIncompleto = "GL-SQL-SCHEMA-002";
    public const string DeploySpFueraDeContexto = "GL-SQL-DEPLOY-010";
    public const string DeployTablaCorteInexistente = "GL-SQL-DEPLOY-011";
    public const string DeployTablaBitacoraInexistente = "GL-SQL-DEPLOY-012";
}
