using System.Security.Cryptography;
using System.Text;
using GestionLudopatas.Api.Domain.Errores;
using GestionLudopatas.Api.Errores;

namespace GestionLudopatas.Api.Security;

/// <summary>
/// Exige <c>X-Api-Key</c> en las 8 operaciones (spec seguridad-acceso-api, Decisión D2 —
/// reemplaza deliberadamente OAuth2 Client Credentials). Comparación de tiempo constante
/// para no filtrar por temporización cuánto de la clave coincide.
/// </summary>
public sealed class ApiKeyAuthenticationMiddleware(
    RequestDelegate siguiente,
    IConfiguration configuracion,
    IHostEnvironment? ambiente = null)
{
    public const string Encabezado = "X-Api-Key";

    public async Task InvokeAsync(HttpContext contexto)
    {
        // /health lo pega el healthcheck de Docker/infra, sin credenciales — igual
        // criterio que el runbook de api-sica (GET /health sin auth).
        if (contexto.Request.Path.StartsWithSegments("/health") || EsDocumentacionDeDesarrollo(contexto.Request.Path))
        {
            await siguiente(contexto);
            return;
        }

        var claveConfigurada = configuracion["Seguridad:ApiKey"];
        if (string.IsNullOrEmpty(claveConfigurada))
            throw new InvalidOperationException("Seguridad:ApiKey no está configurada.");

        if (!contexto.Request.Headers.TryGetValue(Encabezado, out var claveRecibida) ||
            !CoincideEnTiempoConstante(claveRecibida.ToString(), claveConfigurada))
        {
            throw ErrorFuncionalException.DeReglaEspecifica(
                StatusCodes.Status401Unauthorized, CodigoError.AutenticacionRequerida, "Autenticación requerida.");
        }

        await siguiente(contexto);
    }

    private bool EsDocumentacionDeDesarrollo(PathString path) =>
        ambiente?.IsDevelopment() == true &&
        (path.StartsWithSegments("/openapi") || path.StartsWithSegments("/docs"));

    private static bool CoincideEnTiempoConstante(string recibida, string esperada)
    {
        var bytesRecibida = Encoding.UTF8.GetBytes(recibida);
        var bytesEsperada = Encoding.UTF8.GetBytes(esperada);
        return bytesRecibida.Length == bytesEsperada.Length &&
               CryptographicOperations.FixedTimeEquals(bytesRecibida, bytesEsperada);
    }
}

public static class ApiKeyAuthenticationMiddlewareExtensiones
{
    public static IApplicationBuilder UseAutenticacionApiKey(this IApplicationBuilder app) =>
        app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
}
