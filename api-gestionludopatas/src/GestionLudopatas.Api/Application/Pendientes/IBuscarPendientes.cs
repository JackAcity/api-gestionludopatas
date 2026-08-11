namespace GestionLudopatas.Api.Application.Pendientes;

/// <summary>
/// Puerto uniforme (D4) para los 6 SP de pendientes — misma forma, tipos distintos por
/// familia (CALIMACO/CMP comparten request, SICA tiene el suyo; cada item tiene su propio
/// tipo porque el nombre de sus campos difiere por sistema). Solo lectura: los SP no
/// reservan filas ni ordenan — la API no inventa paginación que el SP no tiene.
/// </summary>
public interface IBuscarPendientes<in TRequest, TItem>
{
    Task<IReadOnlyList<TItem>> BuscarAsync(TRequest request, CancellationToken ct);
}

/// <summary>
/// CALIMACO/CMP ingreso y salida comparten exactamente <c>IBuscarPendientes&lt;PendientesCalimacoCmpRequest,TItem&gt;</c>
/// (mismo request, mismo item por familia) — el puerto genérico solo no alcanza para
/// que la inyección de dependencias distinga "el de ingreso" del "de salida" al
/// registrar ambos adaptadores. Estas 4 interfaces marcador (sin miembros propios,
/// heredan el contrato genérico tal cual) dan un tipo único por operación para que el
/// `Manejador*` correspondiente (Application) siga dependiendo solo de una
/// abstracción — nunca del adaptador SQL concreto (DIP) — mientras el contenedor
/// resuelve sin ambigüedad.
/// </summary>
public interface IBuscarPendientesCalimacoIngreso : IBuscarPendientes<PendientesCalimacoCmpRequest, PendienteCalimacoItem>;

public interface IBuscarPendientesCalimacoSalida : IBuscarPendientes<PendientesCalimacoCmpRequest, PendienteCalimacoItem>;

public interface IBuscarPendientesCmpIngreso : IBuscarPendientes<PendientesCalimacoCmpRequest, PendienteCmpItem>;

public interface IBuscarPendientesCmpSalida : IBuscarPendientes<PendientesCalimacoCmpRequest, PendienteCmpItem>;

/// <summary>
/// <c>EsReintentoForzadoEnviado</c> existe porque el contrato distingue "propiedad omitida"
/// (default false, igual al <c>BIT = 0</c> de SQL) de "propiedad enviada como null" (422
/// GL-PEND-*-003). El parser HTTP conserva esa distinción y entrega este request ya tipado
/// al caso de uso, sin introducir tipos de transporte en Application.
/// </summary>
public sealed record PendientesCalimacoCmpRequest(int? CorteIdActual, int? MaxReintentosPorSistema, bool? EsReintentoForzado, bool EsReintentoForzadoEnviado)
;

public sealed record PendientesSicaRequest(int? MaxReintentosPorSistema);

public sealed record PendienteCalimacoItem(
    int Id, string NumeroDocumento, string TipoDocumento, int CorteId, int UltimoCorteId,
    int? CalimacoUltimoCorteId, int CalimacoReintentos);

public sealed record PendienteCmpItem(
    int Id, string NumeroDocumento, string TipoDocumento, int CorteId, int UltimoCorteId,
    int? CmpUltimoCorteId, int CmpReintentos);

/// <summary>SICA ingreso: <c>nombresApellidos</c> nunca nulo (el SP filtra IS NOT NULL).</summary>
public sealed record PendienteSicaIngresoItem(
    int Id, string NumeroDocumento, string TipoDocumento, string NombresApellidos, string? FechaInscripcion,
    int CorteId, int UltimoCorteId, int SicaReintentos);

/// <summary>SICA salida: a diferencia de ingreso, ambos campos pueden ser nulos.</summary>
public sealed record PendienteSicaSalidaItem(
    int Id, string NumeroDocumento, string TipoDocumento, string? NombresApellidos, string? FechaInscripcion,
    int CorteId, int UltimoCorteId, int SicaReintentos);
