using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Vault;

/// <summary>
/// Cliente HTTP mínimo a Vault KV v2 (D8, spec secretos-vault) — mismo patrón que
/// <c>api-sica</c> (openspec/changes/archive/vault-integration/ de ese repo): sin SDK,
/// <c>GET {VAULT_ADDRESS}/v1/secret/data/{path}</c> con header <c>X-Vault-Token</c>,
/// parseo <c>data.data</c>. Genérico a propósito (single responsibility: leer un secreto
/// KV v2, nada más) — no sabe qué es una connection string ni una API Key. La conexión a
/// BD y la API Key viven en paths de Vault SEPARADOS (secretos con dueño y ciclo de
/// rotación distintos: el DBA rota la password, nosotros rotamos la API Key), cada uno
/// leído con una llamada propia a <see cref="ObtenerAsync"/>.
/// </summary>
public sealed class VaultSecretClient(HttpClient httpClient)
{
    public async Task<IReadOnlyDictionary<string, string>> ObtenerAsync(string address, string token, string path, CancellationToken ct)
    {
        using var solicitud = new HttpRequestMessage(HttpMethod.Get, $"{address.TrimEnd('/')}/v1/secret/data/{path}");
        solicitud.Headers.Add("X-Vault-Token", token);

        using var respuesta = await httpClient.SendAsync(solicitud, ct);
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException($"Vault fetch failed: {(int)respuesta.StatusCode}");

        await using var cuerpo = await respuesta.Content.ReadAsStreamAsync(ct);
        using var documento = await JsonDocument.ParseAsync(cuerpo, cancellationToken: ct);
        var campos = documento.RootElement.GetProperty("data").GetProperty("data");

        var resultado = new Dictionary<string, string>();
        foreach (var campo in campos.EnumerateObject())
            if (campo.Value.ValueKind == JsonValueKind.String)
                resultado[campo.Name] = campo.Value.GetString()!;

        return resultado;
    }
}

public static class VaultCampos
{
    public static string Requerido(IReadOnlyDictionary<string, string> campos, string nombre, string path) =>
        campos.TryGetValue(nombre, out var valor)
            ? valor
            : throw new InvalidOperationException($"Vault: falta el campo requerido '{nombre}' en '{path}'.");

    public static void RequerirMotorSqlServer(IReadOnlyDictionary<string, string> campos, string path)
    {
        var engine = Requerido(campos, "engine", path);
        if (!string.Equals(engine, "sqlserver", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Vault: el campo 'engine' en '{path}' debe ser 'sqlserver'.");
    }

    public static string CrearCadenaConexionSqlServer(
        IReadOnlyDictionary<string, string> campos,
        string path,
        bool confiarCertificado)
    {
        RequerirMotorSqlServer(campos, path);

        var connectionString = new SqlConnectionStringBuilder
        {
            DataSource = $"{Requerido(campos, "host", path)},{Requerido(campos, "port", path)}",
            InitialCatalog = Requerido(campos, "dbname", path),
            UserID = Requerido(campos, "username", path),
            Password = Requerido(campos, "password", path),
            Encrypt = true,
            TrustServerCertificate = confiarCertificado,
        };

        return connectionString.ConnectionString;
    }
}
