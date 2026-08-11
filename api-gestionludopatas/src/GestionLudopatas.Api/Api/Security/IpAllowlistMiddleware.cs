using System.Net;
using GestionLudopatas.Api.Domain.Errores;
using GestionLudopatas.Api.Errores;

namespace GestionLudopatas.Api.Security;

/// <summary>
/// Rechaza tráfico fuera de la allowlist configurada (spec seguridad-acceso-api),
/// incluso con API Key válida — segunda barrera, no reemplazo. Se registra después de
/// <see cref="ApiKeyAuthenticationMiddleware"/>. Acepta IPs exactas o rangos CIDR vía
/// <see cref="IPNetwork"/> (nativo desde .NET 8, sin librería adicional).
/// </summary>
public sealed class IpAllowlistMiddleware(RequestDelegate siguiente, IConfiguration configuracion)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        if (contexto.Request.Path.StartsWithSegments("/health"))
        {
            await siguiente(contexto);
            return;
        }

        var permitidas = configuracion.GetSection("Seguridad:IpsPermitidas").Get<string[]>() ?? [];
        var origen = contexto.Connection.RemoteIpAddress;

        if (origen is null || !EstaPermitida(origen, permitidas))
        {
            throw ErrorFuncionalException.DeReglaEspecifica(
                StatusCodes.Status403Forbidden, CodigoError.AccesoDenegado, "La identidad no está autorizada para esta operación.");
        }

        await siguiente(contexto);
    }

    private static bool EstaPermitida(IPAddress origen, IReadOnlyCollection<string> permitidas)
    {
        var normalizado = origen.IsIPv4MappedToIPv6 ? origen.MapToIPv4() : origen;

        foreach (var entrada in permitidas)
        {
            if (IPNetwork.TryParse(entrada, out var red) && red.Contains(normalizado))
                return true;

            if (IPAddress.TryParse(entrada, out var ip) && ip.Equals(normalizado))
                return true;
        }

        return false;
    }
}

public static class IpAllowlistMiddlewareExtensiones
{
    public static IApplicationBuilder UseAllowlistDeIp(this IApplicationBuilder app) =>
        app.UseMiddleware<IpAllowlistMiddleware>();
}
