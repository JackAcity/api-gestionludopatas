namespace GestionLudopatas.Api.Application.Errores;

/// <summary>
/// Fallo de negocio esperado producido por un caso de uso. Conserva los datos estables
/// que el adaptador HTTP traduce, sin construir una respuesta ni depender de mecanismos
/// de transporte o persistencia concretos.
/// </summary>
public sealed record ResultadoError(
    int Status,
    string Codigo,
    string Detalle,
    bool Reintentable,
    string Origen,
    int? SqlErrorNumber = null);
