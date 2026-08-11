using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Errores;

/// <summary>Un renglón de la matriz de adaptación SQL→HTTP del borde API.</summary>
public sealed record ErrorMapeo(int Status, string Codigo, string MensajePublico, bool Reintentable, string Origen);

/// <summary>
/// Traduce números de SQL Server al contrato HTTP. No es una regla de Domain: es la
/// adaptación técnica que consume <c>ManejadorExcepcionesGlobal</c>.
/// </summary>
public static class ErrorMapeoSql
{
    public static readonly IReadOnlyDictionary<int, ErrorMapeo> PorNumero = new Dictionary<int, ErrorMapeo>
    {
        [50000] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.DeployContextoInvalido, "La configuración de persistencia no está disponible.", false, "sql"),
        [50001] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.DeploySchemaCorteIncompleto, "El esquema de persistencia no es compatible con el servicio.", false, "sql"),
        [50002] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.DeploySchemaBitacoraIncompleto, "El esquema de persistencia no es compatible con el servicio.", false, "sql"),
        [50010] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.DeploySpFueraDeContexto, "La configuración de persistencia no está disponible.", false, "sql"),
        [50011] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.DeployTablaCorteInexistente, "El esquema de persistencia requerido no está disponible.", false, "sql"),
        [50012] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.DeployTablaBitacoraInexistente, "El esquema de persistencia requerido no está disponible.", false, "sql"),

        [51000] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.CorteResolverFechaEvaluacionRequerida, "fechaHoraEvaluacion es obligatoria.", false, "sql"),
        [51001] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.CorteResolverTimeoutInvalido, "timeoutMinutos debe ser mayor o igual que cero.", false, "sql"),
        [51002] = new(StatusCodes.Status409Conflict, CodigoError.CorteResolverConflictoOficiales, "El estado actual de cortes oficiales es inconsistente.", false, "sql"),
        [51003] = new(StatusCodes.Status409Conflict, CodigoError.CorteResolverConflictoManuales, "El estado actual de cortes manuales es inconsistente.", false, "sql"),
        [51004] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.CorteResolverTimeoutFueraDeRango, "timeoutMinutos excede el rango permitido para la fecha indicada.", false, "sql"),

        [51010] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.CorteCrearTipoInvalido, "tipoCorte solo permite oficial o manual.", false, "sql"),
        [51011] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.CorteCrearFechaCorteRequerida, "fechaHoraCorte es obligatoria para un corte oficial.", false, "sql"),
        [51012] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.CorteCrearFechaCorteDebeSerNula, "fechaHoraCorte debe omitirse o ser null para un corte manual.", false, "sql"),
        [51013] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.CorteCrearFechaEjecucionRequerida, "fechaHoraEjecucion es obligatoria.", false, "sql"),

        [51100] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCalimacoIngresoCorteInvalido, "corteIdActual debe ser mayor que cero.", false, "sql"),
        [51101] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCalimacoIngresoMaxReintentosInvalido, "maxReintentosPorSistema debe ser mayor que cero.", false, "sql"),
        [51102] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCalimacoIngresoReintentoForzadoInvalido, "esReintentoForzado no puede ser null.", false, "sql"),
        [51110] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCalimacoSalidaCorteInvalido, "corteIdActual debe ser mayor que cero.", false, "sql"),
        [51111] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCalimacoSalidaMaxReintentosInvalido, "maxReintentosPorSistema debe ser mayor que cero.", false, "sql"),
        [51112] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCalimacoSalidaReintentoForzadoInvalido, "esReintentoForzado no puede ser null.", false, "sql"),
        [51120] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCmpIngresoCorteInvalido, "corteIdActual debe ser mayor que cero.", false, "sql"),
        [51121] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCmpIngresoMaxReintentosInvalido, "maxReintentosPorSistema debe ser mayor que cero.", false, "sql"),
        [51122] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCmpIngresoReintentoForzadoInvalido, "esReintentoForzado no puede ser null.", false, "sql"),
        [51130] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCmpSalidaCorteInvalido, "corteIdActual debe ser mayor que cero.", false, "sql"),
        [51131] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCmpSalidaMaxReintentosInvalido, "maxReintentosPorSistema debe ser mayor que cero.", false, "sql"),
        [51132] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendCmpSalidaReintentoForzadoInvalido, "esReintentoForzado no puede ser null.", false, "sql"),
        [51140] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendSicaIngresoMaxReintentosInvalido, "maxReintentosPorSistema debe ser mayor que cero.", false, "sql"),
        [51150] = new(StatusCodes.Status422UnprocessableEntity, CodigoError.PendSicaSalidaMaxReintentosInvalido, "maxReintentosPorSistema debe ser mayor que cero.", false, "sql"),

        [2627] = new(StatusCodes.Status409Conflict, CodigoError.ConflictoDatosPrimaria, "La operación entra en conflicto con un registro existente.", false, "sql"),
        [2601] = new(StatusCodes.Status409Conflict, CodigoError.ConflictoDatosUnicidad, "Ya existe un registro vigente equivalente.", false, "sql"),
        [547] = new(StatusCodes.Status409Conflict, CodigoError.ConflictoDatosReferencial, "La operación referencia un recurso inexistente o incompatible.", false, "sql"),
        [-2] = new(StatusCodes.Status504GatewayTimeout, CodigoError.SqlTimeout, "La persistencia no respondió dentro del tiempo permitido.", true, "sql"),
        [1222] = new(StatusCodes.Status504GatewayTimeout, CodigoError.SqlTimeout, "La persistencia no respondió dentro del tiempo permitido.", true, "sql"),
        [1205] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlDeadlock, "La operación no pudo completarse por contención temporal.", true, "sql"),
        [53] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlNoDisponible, "El servicio de persistencia no está disponible.", true, "sql"),
        [64] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlNoDisponible, "El servicio de persistencia no está disponible.", true, "sql"),
        [233] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlNoDisponible, "El servicio de persistencia no está disponible.", true, "sql"),
        [10053] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlNoDisponible, "El servicio de persistencia no está disponible.", true, "sql"),
        [10054] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlNoDisponible, "El servicio de persistencia no está disponible.", true, "sql"),
        [10060] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlNoDisponible, "El servicio de persistencia no está disponible.", true, "sql"),
        [4060] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlNoDisponible, "El servicio de persistencia no está disponible.", true, "sql"),
        [229] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlSinPermisos, "El servicio de persistencia no está correctamente autorizado.", false, "sql"),
        [262] = new(StatusCodes.Status503ServiceUnavailable, CodigoError.SqlSinPermisos, "El servicio de persistencia no está correctamente autorizado.", false, "sql"),
    };

    public static bool EsErrorDeDespliegue(int sqlErrorNumber) => sqlErrorNumber is >= 50000 and <= 50012;

    public static ErrorMapeo Resolver(int sqlErrorNumber) =>
        PorNumero.TryGetValue(sqlErrorNumber, out var mapeo)
            ? mapeo
            : new ErrorMapeo(StatusCodes.Status500InternalServerError, CodigoError.ApiNoPrevisto, "Ocurrió un error interno no previsto.", false, "sql");
}
