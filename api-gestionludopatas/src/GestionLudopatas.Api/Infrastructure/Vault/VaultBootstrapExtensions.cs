namespace GestionLudopatas.Api.Infrastructure.Vault;

/// <summary>
/// Bootstrap de secretos de Vault (D8, spec secretos-vault) — resuelve la connection
/// string y la API Key ANTES de <c>builder.Build()</c> e inyecta los valores en
/// <see cref="WebApplicationBuilder.Configuration"/>, para que cualquier `AddXxx` que
/// dependa de ellos ya los encuentre resueltos. Fase 1 (dev): <c>Vault:Habilitado=false</c>,
/// no hace nada — la connection string y la API Key salen de appsettings/variables de
/// entorno normales.
/// </summary>
public static class VaultBootstrapExtensions
{
    public static async Task CargarSecretosSiHabilitadoAsync(this WebApplicationBuilder builder)
    {
        if (!builder.Configuration.GetValue<bool>("Vault:Habilitado"))
            return;

        var address = builder.Configuration["Vault:Address"] ?? throw new InvalidOperationException("Vault:Address requerido.");
        var token = builder.Configuration["Vault:Token"] ?? throw new InvalidOperationException("Vault:Token requerido.");
        // Paths separados a propósito (single responsibility): la connection string la rota
        // el DBA, la API Key la rotamos nosotros — no comparten ciclo de vida ni dueño.
        var pathDb = builder.Configuration["Vault:PathDb"] ?? throw new InvalidOperationException("Vault:PathDb requerido.");
        var pathApiKey = builder.Configuration["Vault:PathApiKey"] ?? throw new InvalidOperationException("Vault:PathApiKey requerido.");

        using var httpClienteBootstrap = VaultHttpClientFactory.Crear(builder.Configuration["Vault:RutaCaInterna"]);
        var cliente = new VaultSecretClient(httpClienteBootstrap);

        var camposDb = await cliente.ObtenerAsync(address, token, pathDb, CancellationToken.None);
        var confiarCertificadoSql = builder.Configuration.GetValue<bool>("SqlServer:TrustServerCertificate");
        builder.Configuration["ConnectionStrings:BdAutobot"] =
            VaultCampos.CrearCadenaConexionSqlServer(camposDb, pathDb, confiarCertificadoSql);

        var camposApiKey = await cliente.ObtenerAsync(address, token, pathApiKey, CancellationToken.None);
        builder.Configuration["Seguridad:ApiKey"] = VaultCampos.Requerido(camposApiKey, "api_key", pathApiKey);
    }
}
