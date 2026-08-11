using System.Security.Cryptography.X509Certificates;

namespace GestionLudopatas.Api.Infrastructure.Vault;

/// <summary>
/// Construye el <see cref="HttpClient"/> para hablar con Vault. Nunca deshabilita
/// validación de certificado (spec secretos-vault) — si se configura una CA interna
/// (<c>Vault:RutaCaInterna</c>), se confía explícitamente en ESA CA agregándola al chain
/// de confianza personalizado; sin esa config, se usa la cadena de confianza del sistema
/// sin ningún bypass.
/// </summary>
public static class VaultHttpClientFactory
{
    public static HttpClient Crear(string? rutaCaInterna)
    {
        if (string.IsNullOrEmpty(rutaCaInterna))
            return new HttpClient();

        var caInterna = X509CertificateLoader.LoadCertificateFromFile(rutaCaInterna);
        var manejador = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, certificado, cadena, _) =>
            {
                if (certificado is null || cadena is null)
                    return false;

                cadena.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                cadena.ChainPolicy.CustomTrustStore.Add(caInterna);
                return cadena.Build(certificado);
            },
        };

        return new HttpClient(manejador);
    }
}
