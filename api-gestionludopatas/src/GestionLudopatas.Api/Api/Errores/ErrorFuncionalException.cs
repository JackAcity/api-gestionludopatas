using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Errores;

/// <summary>
/// Error esperado que se origina en el borde HTTP (seguridad, contrato o idempotencia)
/// o al clasificar una excepción de persistencia. Los casos de uso devuelven
/// <c>Result&lt;T&gt;</c> para sus fallos de negocio y no usan esta excepción como flujo normal.
/// </summary>
public sealed class ErrorFuncionalException(int status, string codigo, string detalle, bool reintentable, string origen, int? sqlErrorNumber = null)
    : Exception(detalle)
{
    public int Status { get; } = status;
    public string Codigo { get; } = codigo;
    public string Detalle { get; } = detalle;
    public bool Reintentable { get; } = reintentable;
    public string Origen { get; } = origen;
    public int? SqlErrorNumber { get; } = sqlErrorNumber;

    /// <summary>Regla detectada antes de llegar a un caso de uso o al adaptador.</summary>
    public static ErrorFuncionalException DeReglaEspecifica(int status, string codigo, string detalle, bool reintentable = false) =>
        new(status, codigo, detalle, reintentable, origen: "api");

    /// <summary>Fallback de contrato sin código funcional específico.</summary>
    public static ErrorFuncionalException DeContratoGenerico(string detalle) =>
        new(StatusCodes.Status422UnprocessableEntity, CodigoError.ApiContratoInvalido, detalle, reintentable: false, origen: "api");

    /// <summary>Error de persistencia ya clasificado al contrato HTTP.</summary>
    public static ErrorFuncionalException DeSql(ErrorMapeo mapeo, int sqlErrorNumber) =>
        new(mapeo.Status, mapeo.Codigo, mapeo.MensajePublico, mapeo.Reintentable, mapeo.Origen, sqlErrorNumber);
}
