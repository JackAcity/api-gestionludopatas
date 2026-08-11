using System.Text.Json.Serialization;

namespace GestionLudopatas.Api.Errores;

/// <summary>Cuerpo <c>application/problem+json</c> común a los endpoints HTTP.</summary>
public sealed record ProblemaDetalle
{
    private const string BaseUri = "https://errors.example.invalid/gestion-ludopatas";

    [JsonPropertyName("type")] public required string Type { get; init; }
    [JsonPropertyName("title")] public required string Title { get; init; }
    [JsonPropertyName("status")] public required int Status { get; init; }
    [JsonPropertyName("code")] public required string Code { get; init; }
    [JsonPropertyName("detail")] public required string Detail { get; init; }
    [JsonPropertyName("traceId")] public required string TraceId { get; init; }
    [JsonPropertyName("timestamp")] public required DateTimeOffset Timestamp { get; init; }
    [JsonPropertyName("retryable")] public required bool Retryable { get; init; }
    [JsonPropertyName("source")] public required string Source { get; init; }
    [JsonPropertyName("sqlErrorNumber")] public int? SqlErrorNumber { get; init; }
    [JsonPropertyName("violations")] public IReadOnlyList<string>? Violations { get; init; }

    public static ProblemaDetalle Crear(
        string titulo, int status, string code, string detail, string traceId,
        bool retryable, string source, int? sqlErrorNumber = null, IReadOnlyList<string>? violations = null) =>
        new()
        {
            Type = $"{BaseUri}/{code.ToLowerInvariant()}",
            Title = titulo,
            Status = status,
            Code = code,
            Detail = detail,
            TraceId = traceId,
            Timestamp = DateTimeOffset.UtcNow,
            Retryable = retryable,
            Source = source,
            SqlErrorNumber = sqlErrorNumber,
            Violations = violations,
        };

    /// <summary>Título legible por status HTTP, centralizado para todos los traductores.</summary>
    public static string TituloParaStatus(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Solicitud inválida",
        StatusCodes.Status401Unauthorized => "Autenticación requerida",
        StatusCodes.Status403Forbidden => "Acceso denegado",
        StatusCodes.Status409Conflict => "Conflicto",
        StatusCodes.Status422UnprocessableEntity => "Solicitud no procesable",
        StatusCodes.Status503ServiceUnavailable => "Servicio no disponible",
        StatusCodes.Status504GatewayTimeout => "Tiempo de espera agotado",
        _ => "Error interno",
    };
}
