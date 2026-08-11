using System.Collections.Concurrent;
using GestionLudopatas.Api.Idempotencia;

namespace GestionLudopatas.Api.Infrastructure.Idempotencia;

/// <summary>
/// Almacén de idempotencia de <c>crearCorte</c> (D7, spec corte-crear). Vive en storage
/// propio de la API — nunca en <c>bd_autobot</c> (esa base es solo-EXECUTE, D5).
/// <c>ponytail:</c> en memoria (<see cref="ConcurrentDictionary{TKey,TValue}"/>), no una
/// tabla persistida — se evaluó Microsoft.Data.Sqlite y se descartó por traer
/// SQLitePCLRaw.lib.e_sqlite3 con una vulnerabilidad alta conocida sin versión parcheada
/// limpia (GHSA-2m69-gcr7-jv3q) para una necesidad tan acotada (24h TTL, una sola
/// instancia, un único endpoint). Riesgo aceptado: un reinicio del proceso pierde el
/// registro de idempotencia dentro de esa ventana de 24h — ya documentado en design.md
/// (Risks) como parte del hueco de crash-recovery de <c>crearCorte</c>. Upgrade path: mover a
/// tabla SQL Server propia (no bd_autobot) si el servicio pasa a múltiples instancias o
/// los reinicios en horario de negocio se vuelven frecuentes.
///
/// La reserva y la publicación del resultado son atómicas por clave: dos solicitudes
/// simultáneas nunca ejecutan el SP dos veces en la misma instancia. La segunda espera el
/// resultado de la primera y lo reproduce; con otro fingerprint recibe conflicto.
/// </summary>
public sealed class IdempotencyStore : IIdempotencyStore
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromHours(24);
    private readonly ConcurrentDictionary<string, Entrada> _registros = new();

    public async Task<ResultadoReservaIdempotencia> ReservarAsync(string clave, string fingerprint, CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            if (_registros.TryGetValue(clave, out var existente))
            {
                if (existente is Completada completada)
                {
                    if (DateTimeOffset.UtcNow - completada.CreadoUtc > Vigencia)
                    {
                        EliminarSiEsLaMismaEntrada(clave, completada);
                        continue;
                    }

                    return completada.Registro.Fingerprint == fingerprint
                        ? new ReproducirReservaIdempotencia(completada.Registro)
                        : new ConflictoReservaIdempotencia();
                }

                var enCurso = (EnCurso)existente;
                if (enCurso.Fingerprint != fingerprint)
                    return new ConflictoReservaIdempotencia();

                var registro = await enCurso.Resultado.Task.WaitAsync(ct);
                if (registro is not null)
                    return new ReproducirReservaIdempotencia(registro);

                // La primera ejecución no produjo una respuesta exitosa; la clave queda libre.
                continue;
            }

            var candidata = new EnCurso(fingerprint);
            if (_registros.TryAdd(clave, candidata))
                return new EjecutarReservaIdempotencia(new ReservaIdempotencia(clave, candidata.Token));
        }
    }

    public void Completar(ReservaIdempotencia reserva, int status, string responseBodyJson)
    {
        if (!_registros.TryGetValue(reserva.Clave, out var existente)
            || existente is not EnCurso enCurso
            || enCurso.Token != reserva.Token)
            throw new InvalidOperationException("La reserva de idempotencia ya no pertenece a esta ejecución.");

        var registro = new RegistroIdempotencia(enCurso.Fingerprint, status, responseBodyJson);
        var completada = new Completada(registro, DateTimeOffset.UtcNow);
        if (!_registros.TryUpdate(reserva.Clave, completada, enCurso))
            throw new InvalidOperationException("No se pudo publicar el resultado de idempotencia.");

        enCurso.Resultado.TrySetResult(registro);
    }

    public void Cancelar(ReservaIdempotencia reserva)
    {
        if (!_registros.TryGetValue(reserva.Clave, out var existente)
            || existente is not EnCurso enCurso
            || enCurso.Token != reserva.Token
            || !EliminarSiEsLaMismaEntrada(reserva.Clave, enCurso))
            return;

        enCurso.Resultado.TrySetResult(null);
    }

    private bool EliminarSiEsLaMismaEntrada(string clave, Entrada entrada) =>
        ((ICollection<KeyValuePair<string, Entrada>>)_registros).Remove(new KeyValuePair<string, Entrada>(clave, entrada));

    private abstract record Entrada(string Fingerprint);

    private sealed record Completada(RegistroIdempotencia Registro, DateTimeOffset CreadoUtc)
        : Entrada(Registro.Fingerprint);

    private sealed record EnCurso(string Fingerprint) : Entrada(Fingerprint)
    {
        public Guid Token { get; } = Guid.NewGuid();
        public TaskCompletionSource<RegistroIdempotencia?> Resultado { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
