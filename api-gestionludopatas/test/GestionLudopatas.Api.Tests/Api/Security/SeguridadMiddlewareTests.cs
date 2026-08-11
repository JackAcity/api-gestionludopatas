using System.Net;
using GestionLudopatas.Api.Domain.Errores;
using GestionLudopatas.Api.Errores;
using GestionLudopatas.Api.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GestionLudopatas.Api.Tests.Security;

/// <summary>Tarea 9.5 — ApiKeyAuthenticationMiddleware / IpAllowlistMiddleware en aislado.</summary>
public class SeguridadMiddlewareTests
{
    private static IConfiguration Config(params (string Key, string? Value)[] valores) =>
        new ConfigurationBuilder().AddInMemoryCollection(valores.ToDictionary(v => v.Key, v => v.Value)).Build();

    private static (RequestDelegate Siguiente, Func<bool> FueLlamado) Espia()
    {
        var llamado = false;
        RequestDelegate siguiente = _ => { llamado = true; return Task.CompletedTask; };
        return (siguiente, () => llamado);
    }

    private static IHostEnvironment Ambiente(string nombre) => new AmbientePrueba { EnvironmentName = nombre };

    private sealed class AmbientePrueba : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = nameof(GestionLudopatas);
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    [Fact]
    public async Task ApiKey_ausente_lanza_401_GL_AUTH_001_y_no_llama_al_siguiente()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new ApiKeyAuthenticationMiddleware(siguiente, Config(("Seguridad:ApiKey", "clave-correcta")));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/cortes";

        var ex = await Assert.ThrowsAsync<ErrorFuncionalException>(() => middleware.InvokeAsync(contexto));

        Assert.Equal(CodigoError.AutenticacionRequerida, ex.Codigo);
        Assert.Equal(StatusCodes.Status401Unauthorized, ex.Status);
        Assert.False(fueLlamado());
    }

    [Fact]
    public async Task ApiKey_invalida_lanza_401()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new ApiKeyAuthenticationMiddleware(siguiente, Config(("Seguridad:ApiKey", "clave-correcta")));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/cortes";
        contexto.Request.Headers[ApiKeyAuthenticationMiddleware.Encabezado] = "clave-incorrecta";

        await Assert.ThrowsAsync<ErrorFuncionalException>(() => middleware.InvokeAsync(contexto));
        Assert.False(fueLlamado());
    }

    [Fact]
    public async Task ApiKey_valida_llama_al_siguiente()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new ApiKeyAuthenticationMiddleware(siguiente, Config(("Seguridad:ApiKey", "clave-correcta")));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/cortes";
        contexto.Request.Headers[ApiKeyAuthenticationMiddleware.Encabezado] = "clave-correcta";

        await middleware.InvokeAsync(contexto);
        Assert.True(fueLlamado());
    }

    [Fact]
    public async Task ApiKey_health_no_requiere_credenciales()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new ApiKeyAuthenticationMiddleware(siguiente, Config(("Seguridad:ApiKey", "clave-correcta")));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/health";

        await middleware.InvokeAsync(contexto);
        Assert.True(fueLlamado());
    }

    [Fact]
    public async Task ApiKey_documentacion_en_development_no_requiere_credenciales_pero_sigue_en_pipeline()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new ApiKeyAuthenticationMiddleware(
            siguiente, Config(("Seguridad:ApiKey", "clave-correcta")), Ambiente(Environments.Development));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/openapi/v1.json";

        await middleware.InvokeAsync(contexto);

        Assert.True(fueLlamado());
    }

    [Fact]
    public async Task ApiKey_documentacion_fuera_de_development_requiere_credenciales()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new ApiKeyAuthenticationMiddleware(
            siguiente, Config(("Seguridad:ApiKey", "clave-correcta")), Ambiente("QA"));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/docs";

        await Assert.ThrowsAsync<ErrorFuncionalException>(() => middleware.InvokeAsync(contexto));

        Assert.False(fueLlamado());
    }

    [Fact]
    public async Task IpAllowlist_ip_fuera_de_lista_lanza_403_GL_AUTH_002()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new IpAllowlistMiddleware(siguiente, Config(("Seguridad:IpsPermitidas:0", "10.0.0.5")));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/cortes";
        contexto.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.9");

        var ex = await Assert.ThrowsAsync<ErrorFuncionalException>(() => middleware.InvokeAsync(contexto));

        Assert.Equal(CodigoError.AccesoDenegado, ex.Codigo);
        Assert.Equal(StatusCodes.Status403Forbidden, ex.Status);
        Assert.False(fueLlamado());
    }

    [Fact]
    public async Task IpAllowlist_ip_exacta_permitida_llama_al_siguiente()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new IpAllowlistMiddleware(siguiente, Config(("Seguridad:IpsPermitidas:0", "10.0.0.5")));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/cortes";
        contexto.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.5");

        await middleware.InvokeAsync(contexto);
        Assert.True(fueLlamado());
    }

    [Fact]
    public async Task IpAllowlist_ip_en_segunda_entrada_permitida_llama_al_siguiente()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new IpAllowlistMiddleware(
            siguiente,
            Config(("Seguridad:IpsPermitidas:0", "10.0.0.5"), ("Seguridad:IpsPermitidas:1", "10.0.0.9")));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/cortes";
        contexto.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.9");

        await middleware.InvokeAsync(contexto);

        Assert.True(fueLlamado());
    }

    [Fact]
    public async Task IpAllowlist_cidr_permite_ips_del_rango()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new IpAllowlistMiddleware(siguiente, Config(("Seguridad:IpsPermitidas:0", "10.0.0.0/24")));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/cortes";
        contexto.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.200");

        await middleware.InvokeAsync(contexto);
        Assert.True(fueLlamado());
    }

    [Fact]
    public async Task IpAllowlist_health_no_se_filtra()
    {
        var (siguiente, fueLlamado) = Espia();
        var middleware = new IpAllowlistMiddleware(siguiente, Config(("Seguridad:IpsPermitidas:0", "10.0.0.5")));
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/health";
        contexto.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.1");

        await middleware.InvokeAsync(contexto);
        Assert.True(fueLlamado());
    }
}
