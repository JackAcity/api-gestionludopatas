using System.Reflection;
using System.Net.Http;
using GestionLudopatas.Api.Application.Resultados;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Tests.Arquitectura;

public class ArquitecturaTests
{
    private const string NombreMiembroInfractorSql = "ConexionSql";
    private const string NombreMiembroInfractorHttp = "ContextoHttp";

    private static readonly string[] NamespacesProhibidos =
    [
        "Microsoft.Data.SqlClient",
        "System.Data.SqlClient",
        "Npgsql",
        "Microsoft.EntityFrameworkCore",
        "RabbitMQ.Client"
    ];

    // ARCH-002: StatusCodes aislado se permite en el perfil pragmático, pero estos
    // mecanismos convierten Application/Domain en un adaptador HTTP y se prohíben.
    private static readonly Type[] TiposWebProhibidos =
    [
        typeof(HttpContext),
        typeof(HttpRequest),
        typeof(HttpResponse),
        typeof(IResult),
        typeof(IEndpointRouteBuilder),
        typeof(HttpClient)
    ];

    private static readonly string[] SimbolosDeBordeProhibidosEnCodigo =
    [
        "HttpContext",
        "IResult",
        "IEndpointRouteBuilder",
        "HttpClient",
        "Results.",
        "ProblemDetails"
    ];

    private const BindingFlags MiembrosDeclarados =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static |
        BindingFlags.DeclaredOnly;

    [Fact]
    public void Application_y_Domain_no_referencian_infraestructura_concreta()
    {
        var tiposDelNucleo = typeof(Result<>).Assembly
            .GetTypes()
            .Where(EsTipoDelNucleo);

        var infracciones = BuscarInfracciones(tiposDelNucleo);

        Assert.True(infracciones.Count == 0, FormatearInfracciones(infracciones));
    }

    [Fact]
    public void Application_y_Domain_no_usan_simbolos_de_borde_en_codigo()
    {
        var infracciones = BuscarSimbolosDeBordeEnCodigo();

        Assert.True(infracciones.Count == 0, FormatearSimbolosDeBorde(infracciones));
    }

    [Fact]
    public void La_guardia_identifica_tipo_y_miembro_infractor()
    {
        var infracciones = BuscarInfracciones([typeof(TipoDePruebaConDependenciaSql)]);

        var infraccion = Assert.Single(infracciones, infraccion => infraccion.Miembro == NombreMiembroInfractorSql);

        Assert.Equal(typeof(TipoDePruebaConDependenciaSql).FullName, infraccion.TipoDeclarante);
        Assert.Equal(NombreMiembroInfractorSql, infraccion.Miembro);
        Assert.Equal(typeof(SqlConnection), infraccion.TipoProhibido);
        Assert.Contains(nameof(TipoDePruebaConDependenciaSql), FormatearInfracciones(infracciones));
        Assert.Contains(NombreMiembroInfractorSql, FormatearInfracciones(infracciones));
    }

    [Fact]
    public void La_guardia_identifica_tipo_y_miembro_http_infractor()
    {
        var infracciones = BuscarInfracciones([typeof(TipoDePruebaConDependenciaHttp)]);

        var infraccion = Assert.Single(infracciones, infraccion => infraccion.Miembro == NombreMiembroInfractorHttp);

        Assert.Equal(typeof(TipoDePruebaConDependenciaHttp).FullName, infraccion.TipoDeclarante);
        Assert.Equal(typeof(HttpContext), infraccion.TipoProhibido);
    }

    [Fact]
    public void La_guardia_identifica_llamada_estatica_de_borde_en_codigo_del_nucleo()
    {
        var infracciones = BuscarSimbolosDeBordeEnContenido(
            "Application/Ficticio.cs",
            "internal static IResult Responder() => Results.Ok();");

        Assert.Contains(infracciones, infraccion =>
            infraccion.Archivo == "Application/Ficticio.cs" && infraccion.Simbolo == "Results.");
    }

    private static bool EsTipoDelNucleo(Type tipo) =>
        tipo.Namespace?.StartsWith("GestionLudopatas.Api.Application", StringComparison.Ordinal) == true ||
        tipo.Namespace?.StartsWith("GestionLudopatas.Api.Domain", StringComparison.Ordinal) == true;

    private static List<InfraccionArquitectonica> BuscarInfracciones(IEnumerable<Type> tipos) =>
        tipos.SelectMany(BuscarInfracciones)
            .OrderBy(infraccion => infraccion.TipoDeclarante, StringComparer.Ordinal)
            .ThenBy(infraccion => infraccion.Miembro, StringComparer.Ordinal)
            .ToList();

    private static IEnumerable<InfraccionArquitectonica> BuscarInfracciones(Type tipo)
    {
        foreach (var constructor in tipo.GetConstructors(MiembrosDeclarados))
        {
            foreach (var parametro in constructor.GetParameters())
            {
                if (EncontrarTipoProhibido(parametro.ParameterType) is { } tipoProhibido)
                {
                    yield return new InfraccionArquitectonica(tipo.FullName!, $".ctor({parametro.Name})", tipoProhibido);
                }
            }
        }

        foreach (var metodo in tipo.GetMethods(MiembrosDeclarados))
        {
            if (EncontrarTipoProhibido(metodo.ReturnType) is { } tipoRetornoProhibido)
            {
                yield return new InfraccionArquitectonica(tipo.FullName!, $"{metodo.Name} retorno", tipoRetornoProhibido);
            }

            foreach (var parametro in metodo.GetParameters())
            {
                if (EncontrarTipoProhibido(parametro.ParameterType) is { } tipoParametroProhibido)
                {
                    yield return new InfraccionArquitectonica(tipo.FullName!, $"{metodo.Name}({parametro.Name})", tipoParametroProhibido);
                }
            }
        }

        foreach (var propiedad in tipo.GetProperties(MiembrosDeclarados))
        {
            if (EncontrarTipoProhibido(propiedad.PropertyType) is { } tipoProhibido)
            {
                yield return new InfraccionArquitectonica(tipo.FullName!, propiedad.Name, tipoProhibido);
            }
        }

        foreach (var campo in tipo.GetFields(MiembrosDeclarados))
        {
            if (EncontrarTipoProhibido(campo.FieldType) is { } tipoProhibido)
            {
                yield return new InfraccionArquitectonica(tipo.FullName!, campo.Name, tipoProhibido);
            }
        }
    }

    private static Type? EncontrarTipoProhibido(Type tipo)
    {
        if (PerteneceANamespaceProhibido(tipo) || TiposWebProhibidos.Contains(tipo))
        {
            return tipo;
        }

        if (tipo.HasElementType)
        {
            return EncontrarTipoProhibido(tipo.GetElementType()!);
        }

        return tipo.IsGenericType
            ? tipo.GetGenericArguments()
                .Select(EncontrarTipoProhibido)
                .FirstOrDefault(tipoProhibido => tipoProhibido is not null)
            : null;
    }

    private static List<SimboloDeBordeEnCodigo> BuscarSimbolosDeBordeEnCodigo()
    {
        var raiz = EncontrarRaizRepositorio();
        var directoriosNucleo = new[]
        {
            Path.Combine(raiz, "src", "GestionLudopatas.Api", "Application"),
            Path.Combine(raiz, "src", "GestionLudopatas.Api", "Domain")
        };

        return directoriosNucleo
            .SelectMany(directorio => Directory.EnumerateFiles(directorio, "*.cs", SearchOption.AllDirectories))
            .SelectMany(archivo => BuscarSimbolosDeBordeEnContenido(
                Path.GetRelativePath(raiz, archivo),
                File.ReadAllText(archivo)))
            .OrderBy(infraccion => infraccion.Archivo, StringComparer.Ordinal)
            .ThenBy(infraccion => infraccion.Simbolo, StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<SimboloDeBordeEnCodigo> BuscarSimbolosDeBordeEnContenido(string archivo, string contenido) =>
        SimbolosDeBordeProhibidosEnCodigo
            .Where(simbolo => contenido.Contains(simbolo, StringComparison.Ordinal))
            .Select(simbolo => new SimboloDeBordeEnCodigo(archivo, simbolo));

    private static string EncontrarRaizRepositorio()
    {
        for (var directorio = new DirectoryInfo(AppContext.BaseDirectory); directorio is not null; directorio = directorio.Parent)
        {
            if (File.Exists(Path.Combine(directorio.FullName, "api-gestionludopatas.slnx")))
                return directorio.FullName;
        }

        throw new DirectoryNotFoundException("No se encontró la raíz de api-gestionludopatas para la guardia de arquitectura.");
    }

    private static bool PerteneceANamespaceProhibido(Type tipo) =>
        tipo.Namespace is not null &&
        NamespacesProhibidos.Any(namespaceProhibido =>
            tipo.Namespace.Equals(namespaceProhibido, StringComparison.Ordinal) ||
            tipo.Namespace.StartsWith($"{namespaceProhibido}.", StringComparison.Ordinal));

    private static string FormatearInfracciones(IReadOnlyCollection<InfraccionArquitectonica> infracciones) =>
        $"Application/Domain referencia infraestructura concreta:{Environment.NewLine}" +
        string.Join(Environment.NewLine, infracciones.Select(infraccion =>
            $"- {infraccion.TipoDeclarante}.{infraccion.Miembro} -> {infraccion.TipoProhibido.FullName}"));

    private static string FormatearSimbolosDeBorde(IReadOnlyCollection<SimboloDeBordeEnCodigo> infracciones) =>
        $"Application/Domain usa símbolos de borde prohibidos:{Environment.NewLine}" +
        string.Join(Environment.NewLine, infracciones.Select(infraccion =>
            $"- {infraccion.Archivo} -> {infraccion.Simbolo}"));

    private sealed record InfraccionArquitectonica(string TipoDeclarante, string Miembro, Type TipoProhibido);
    private sealed record SimboloDeBordeEnCodigo(string Archivo, string Simbolo);

    private sealed class TipoDePruebaConDependenciaSql
    {
        private SqlConnection ConexionSql { get; } = null!;
    }

    private sealed class TipoDePruebaConDependenciaHttp
    {
        private HttpContext ContextoHttp { get; } = null!;
    }
}
