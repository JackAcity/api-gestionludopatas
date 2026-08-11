using System.Runtime.CompilerServices;
using Xunit;

namespace GestionLudopatas.Api.Tests.Infrastructure;

/// <summary>
/// Guardia estructural (change gestion-ludopatas-limpieza-estructura): sin carpetas
/// vacías bajo el código fuente, y <c>Program.cs</c> como composition root puro sin
/// bootstrap de Vault inline. Resuelve la ruta a <c>src/GestionLudopatas.Api</c> desde
/// la ubicación en disco de este propio archivo de test (<see cref="CallerFilePathAttribute"/>),
/// no desde el directorio de salida del build.
/// </summary>
public class EstructuraProyectoTests
{
    private static readonly string RutaProyectoFuente = ResolverRutaProyectoFuente();

    private static string ResolverRutaProyectoFuente([CallerFilePath] string archivoActual = "")
    {
        var directorioTest = Path.GetDirectoryName(archivoActual)!;
        return Path.GetFullPath(Path.Combine(directorioTest, "..", "..", "..", "src", "GestionLudopatas.Api"));
    }

    [Fact]
    public void No_deberian_existir_carpetas_vacias_en_src()
    {
        Assert.True(Directory.Exists(RutaProyectoFuente), $"No se encontró el proyecto fuente en '{RutaProyectoFuente}'.");

        var carpetasVacias = Directory.EnumerateDirectories(RutaProyectoFuente, "*", SearchOption.AllDirectories)
            .Where(carpeta => !carpeta.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                               !carpeta.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                               !carpeta.EndsWith($"{Path.DirectorySeparatorChar}bin") &&
                               !carpeta.EndsWith($"{Path.DirectorySeparatorChar}obj"))
            .Where(carpeta => Directory.GetFileSystemEntries(carpeta).Length == 0)
            .ToList();

        Assert.True(carpetasVacias.Count == 0,
            $"Carpetas vacías encontradas bajo src/GestionLudopatas.Api: {string.Join(", ", carpetasVacias)}");
    }

    [Fact]
    public void Program_cs_no_contiene_bootstrap_de_vault_inline()
    {
        var rutaProgram = Path.Combine(RutaProyectoFuente, "Program.cs");
        Assert.True(File.Exists(rutaProgram), $"No se encontró '{rutaProgram}'.");

        var contenido = File.ReadAllText(rutaProgram);

        Assert.DoesNotContain("Vault:Address", contenido);
        Assert.DoesNotContain("new VaultSecretClient", contenido);
        Assert.Contains("CargarSecretosSiHabilitadoAsync", contenido);
    }
}
