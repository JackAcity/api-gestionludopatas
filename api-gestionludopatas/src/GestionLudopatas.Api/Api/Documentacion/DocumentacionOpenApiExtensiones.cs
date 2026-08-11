using GestionLudopatas.Api.Security;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace GestionLudopatas.Api.Documentacion;

/// <summary>
/// Expone la referencia interactiva exclusivamente en Development. La API key se omite
/// solo para estas rutas de documentación porque un navegador no puede adjuntarla antes
/// de cargar la UI; la allowlist de IP sigue siendo obligatoria en el pipeline. Scalar
/// no persiste la autenticación y no habilita Agent ni fuentes externas.
/// </summary>
public static class DocumentacionOpenApiExtensiones
{
    public static IServiceCollection AddDocumentacionOpenApi(this IServiceCollection servicios)
    {
        servicios.AddOpenApi(opciones => opciones.AddDocumentTransformer((documento, _, _) =>
        {
            documento.Components ??= new OpenApiComponents();
            documento.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["ApiKey"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Name = ApiKeyAuthenticationMiddleware.Encabezado,
                    In = ParameterLocation.Header
                }
            };

            foreach (var operacion in documento.Paths.Values.SelectMany(ruta => ruta.Operations ?? []))
            {
                operacion.Value.Security ??= [];
                operacion.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("ApiKey", documento)] = []
                });
            }

            return Task.CompletedTask;
        }));
        return servicios;
    }

    public static void MapDocumentacionOpenApiSoloDesarrollo(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        app.MapOpenApi();
        app.MapScalarApiReference("/docs", opciones => opciones
            .WithTitle("GestionLudopatas.Api")
            .DisableAgent()
            .DisableDefaultFonts());
    }
}
