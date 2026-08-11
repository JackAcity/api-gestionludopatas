using System.Net;
using System.Text;
using GestionLudopatas.Api.Infrastructure.Vault;
using Microsoft.Data.SqlClient;
using Xunit;

namespace GestionLudopatas.Api.Tests.Infrastructure.Vault;

/// <summary>Tarea 9.6 — VaultSecretClient con HttpMessageHandler fake, sin Vault real (mismo formato que vault.util.spec.ts de api-sica).</summary>
public class VaultSecretClientTests
{
    private sealed class HandlerFake(HttpStatusCode status, string cuerpo) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(cuerpo, Encoding.UTF8, "application/json") });
    }

    private const string CuerpoDb = """
        {"data":{"data":{"engine":"sqlserver","host":"10.0.0.1","port":"1433","dbname":"bd_autobot","username":"app_api_rw","password":"secreta"}}}
        """;

    private const string CuerpoApiKey = """{"data":{"data":{"api_key":"la-api-key"}}}""";

    [Fact] // V-01
    public async Task Respuesta_200_devuelve_los_campos_del_path_solicitado()
    {
        var cliente = new VaultSecretClient(new HttpClient(new HandlerFake(HttpStatusCode.OK, CuerpoDb)));

        var campos = await cliente.ObtenerAsync("https://vault.invalid", "token", "api-gestionludopatas/dev/db", CancellationToken.None);

        Assert.Equal("sqlserver", campos["engine"]);
        Assert.Equal("10.0.0.1", campos["host"]);
        Assert.Equal("1433", campos["port"]);
        Assert.Equal("bd_autobot", campos["dbname"]);
        Assert.Equal("app_api_rw", campos["username"]);
        Assert.Equal("secreta", campos["password"]);
    }

    [Fact] // paths separados (single responsibility) — el path de api_key no trae campos de BD
    public async Task Path_de_api_key_devuelve_solo_ese_campo()
    {
        var cliente = new VaultSecretClient(new HttpClient(new HandlerFake(HttpStatusCode.OK, CuerpoApiKey)));

        var campos = await cliente.ObtenerAsync("https://vault.invalid", "token", "api-gestionludopatas/dev/apikey", CancellationToken.None);

        Assert.Equal("la-api-key", campos["api_key"]);
        Assert.False(campos.ContainsKey("host"));
    }

    [Fact] // V-02
    public async Task Respuesta_403_lanza_excepcion_con_403_en_el_mensaje()
    {
        var cliente = new VaultSecretClient(new HttpClient(new HandlerFake(HttpStatusCode.Forbidden, "{}")));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            cliente.ObtenerAsync("https://vault.invalid", "token-invalido", "api-gestionludopatas/dev/db", CancellationToken.None));

        Assert.Contains("403", ex.Message);
    }

    [Fact] // V-03
    public async Task Respuesta_404_lanza_excepcion_con_404_en_el_mensaje()
    {
        var cliente = new VaultSecretClient(new HttpClient(new HandlerFake(HttpStatusCode.NotFound, "{}")));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            cliente.ObtenerAsync("https://vault.invalid", "token", "path/inexistente", CancellationToken.None));

        Assert.Contains("404", ex.Message);
    }

    [Fact] // V-04 / V-06 — VaultCampos.Requerido lanza si el campo no vino en la respuesta
    public void VaultCampos_Requerido_lanza_si_falta_el_campo()
    {
        var campos = new Dictionary<string, string> { ["port"] = "1433" };

        var ex = Assert.Throws<InvalidOperationException>(() => VaultCampos.Requerido(campos, "host", "api-gestionludopatas/dev/db"));

        Assert.Contains("host", ex.Message);
    }

    [Fact]
    public void VaultCampos_RequerirMotorSqlServer_acepta_el_motor_estandar()
    {
        var campos = new Dictionary<string, string> { ["engine"] = "sqlserver" };

        VaultCampos.RequerirMotorSqlServer(campos, "api-gestionludopatas/db");
    }

    [Theory]
    [InlineData("postgres")]
    [InlineData("mysql")]
    public void VaultCampos_RequerirMotorSqlServer_lanza_si_el_motor_no_es_sqlserver(string engine)
    {
        var campos = new Dictionary<string, string> { ["engine"] = engine };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            VaultCampos.RequerirMotorSqlServer(campos, "api-gestionludopatas/db"));

        Assert.Contains("engine", ex.Message);
        Assert.Contains("sqlserver", ex.Message);
    }

    [Fact]
    public void VaultCampos_RequerirMotorSqlServer_lanza_si_falta_engine()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            VaultCampos.RequerirMotorSqlServer(new Dictionary<string, string>(), "api-gestionludopatas/db"));

        Assert.Contains("engine", ex.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VaultCampos_CrearCadenaConexionSqlServer_preserva_la_politica_de_certificado(bool confiarCertificado)
    {
        var campos = new Dictionary<string, string>
        {
            ["engine"] = "sqlserver",
            ["host"] = "servidor",
            ["port"] = "1473",
            ["dbname"] = "bd_autobot",
            ["username"] = "usuario",
            ["password"] = "clave;con;separadores",
        };

        var cadena = VaultCampos.CrearCadenaConexionSqlServer(campos, "api-gestionludopatas/db", confiarCertificado);
        var builder = new SqlConnectionStringBuilder(cadena);

        Assert.Equal("servidor,1473", builder.DataSource);
        Assert.Equal("bd_autobot", builder.InitialCatalog);
        Assert.Equal("usuario", builder.UserID);
        Assert.Equal("clave;con;separadores", builder.Password);
        Assert.True(builder.Encrypt);
        Assert.Equal(confiarCertificado, builder.TrustServerCertificate);
    }

    [Fact] // V-05
    public async Task Red_inaccesible_propaga_la_excepcion_sin_silenciarla()
    {
        var cliente = new VaultSecretClient(new HttpClient(new HandlerLanzaExcepcionDeRed()));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            cliente.ObtenerAsync("https://vault.invalid", "token", "api-gestionludopatas/dev/db", CancellationToken.None));
    }

    private sealed class HandlerLanzaExcepcionDeRed : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            throw new HttpRequestException("red inaccesible");
    }
}
