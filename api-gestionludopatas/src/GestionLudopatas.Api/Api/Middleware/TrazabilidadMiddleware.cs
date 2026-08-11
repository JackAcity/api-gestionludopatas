namespace GestionLudopatas.Api.Middleware;

/// <summary>
/// <c>X-Trace-Id</c> obligatorio de salida (usa el <see cref="HttpContext.TraceIdentifier"/>
/// que ASP.NET Core ya genera por request) y <c>X-Correlation-Id</c> de entrada propagado
/// tal cual si el cliente lo envió (spec modelo-error-comun).
/// </summary>
public sealed class TrazabilidadMiddleware(RequestDelegate siguiente)
{
    public const string EncabezadoTraceId = "X-Trace-Id";
    public const string EncabezadoCorrelationId = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext contexto)
    {
        contexto.Response.OnStarting(() =>
        {
            contexto.Response.Headers[EncabezadoTraceId] = contexto.TraceIdentifier;

            if (contexto.Request.Headers.TryGetValue(EncabezadoCorrelationId, out var correlationId))
                contexto.Response.Headers[EncabezadoCorrelationId] = correlationId;

            return Task.CompletedTask;
        });

        await siguiente(contexto);
    }
}

public static class TrazabilidadMiddlewareExtensiones
{
    public static IApplicationBuilder UseTrazabilidad(this IApplicationBuilder app) =>
        app.UseMiddleware<TrazabilidadMiddleware>();
}
