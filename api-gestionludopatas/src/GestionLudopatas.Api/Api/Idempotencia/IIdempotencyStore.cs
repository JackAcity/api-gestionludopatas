namespace GestionLudopatas.Api.Idempotencia;

/// <summary>Respuesta HTTP exitosa que puede reproducirse para una clave idempotente.</summary>
public sealed record RegistroIdempotencia(string Fingerprint, int Status, string ResponseBodyJson);

/// <summary>Resultado exclusivo de intentar adquirir una clave de idempotencia.</summary>
public abstract record ResultadoReservaIdempotencia;

/// <summary>La solicitud actual posee la clave y debe ejecutar el caso de uso.</summary>
public sealed record EjecutarReservaIdempotencia(ReservaIdempotencia Reserva) : ResultadoReservaIdempotencia;

/// <summary>Ya existe una ejecución exitosa con el mismo payload y debe reproducirse.</summary>
public sealed record ReproducirReservaIdempotencia(RegistroIdempotencia Registro) : ResultadoReservaIdempotencia;

/// <summary>La clave ya pertenece a un payload distinto.</summary>
public sealed record ConflictoReservaIdempotencia : ResultadoReservaIdempotencia;

/// <summary>Token opaco que identifica la ejecución propietaria de una clave.</summary>
public sealed record ReservaIdempotencia(string Clave, Guid Token);

/// <summary>
/// Puerto de almacenamiento de idempotencia del borde HTTP. Conserva el contrato
/// necesario para reproducir respuestas sin acoplar el endpoint a una tecnología de
/// persistencia concreta.
/// </summary>
public interface IIdempotencyStore
{
    Task<ResultadoReservaIdempotencia> ReservarAsync(string clave, string fingerprint, CancellationToken ct);

    void Completar(ReservaIdempotencia reserva, int status, string responseBodyJson);

    void Cancelar(ReservaIdempotencia reserva);
}
