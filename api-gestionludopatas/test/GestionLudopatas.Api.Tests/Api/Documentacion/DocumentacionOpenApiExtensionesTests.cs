using System.Text.Json;
using GestionLudopatas.Api.Documentacion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GestionLudopatas.Api.Tests.Documentacion;

public sealed class DocumentacionOpenApiExtensionesTests
{
    [Fact]
    public async Task Development_mapea_documento_y_referencia_interactiva()
    {
        var app = CrearAplicacion(Environments.Development);
        try
        {
            var rutas = Rutas(app);

            Assert.Contains(rutas, ruta => ruta.StartsWith("/openapi/", StringComparison.Ordinal));
            Assert.Contains(rutas, ruta => ruta.StartsWith("/docs", StringComparison.Ordinal));
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task Development_declara_api_key_en_header_para_las_operaciones()
    {
        var app = CrearAplicacion(Environments.Development);
        try
        {
            app.Urls.Add("http://127.0.0.1:0");
            await app.StartAsync();
            var servidor = app.Services.GetRequiredService<IServer>();
            var direccion = servidor.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var cliente = new HttpClient { BaseAddress = new Uri(direccion) };
            using var documento = JsonDocument.Parse(await cliente.GetStringAsync("/openapi/v1.json"));
            var raiz = documento.RootElement;
            var esquema = raiz.GetProperty("components").GetProperty("securitySchemes").GetProperty("ApiKey");

            Assert.Equal("apiKey", esquema.GetProperty("type").GetString());
            Assert.Equal("X-Api-Key", esquema.GetProperty("name").GetString());
            Assert.Equal("header", esquema.GetProperty("in").GetString());
            foreach (var ruta in raiz.GetProperty("paths").EnumerateObject())
            {
                foreach (var operacion in ruta.Value.EnumerateObject())
                {
                    Assert.NotEmpty(operacion.Value.GetProperty("security").EnumerateArray());
                }
            }
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    [Theory]
    [InlineData("QA")]
    [InlineData("Production")]
    public async Task Fuera_de_development_no_mapea_documentacion(string ambiente)
    {
        var app = CrearAplicacion(ambiente);
        try
        {
            Assert.Empty(Rutas(app));
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    private static WebApplication CrearAplicacion(string ambiente)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = ambiente });
        builder.Services.AddDocumentacionOpenApi();
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
            app.MapPost("/api/prueba", () => Results.Ok());
        app.MapDocumentacionOpenApiSoloDesarrollo();
        return app;
    }

    private static IReadOnlyCollection<string> Rutas(WebApplication app) =>
        ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(fuente => fuente.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .Where(ruta => ruta is not null)
            .Cast<string>()
            .ToArray();
}
