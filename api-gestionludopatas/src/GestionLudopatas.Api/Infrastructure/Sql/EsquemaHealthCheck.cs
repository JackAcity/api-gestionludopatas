using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>
/// Verifica que los 8 SP y las 2 tablas que el contrato necesita existan en
/// <c>bd_autobot</c> antes de servir tráfico (D6). No migra ni modifica el esquema —
/// `bd_autobot` es administrada externamente. Si el esquema no es compatible, el
/// healthcheck reporta <see cref="HealthStatus.Unhealthy"/>; nunca se traduce esto en
/// una respuesta runtime de un endpoint de negocio (los códigos GL-SQL-DEPLOY-*/GL-SQL-SCHEMA-*
/// no son alcanzables desde un handler HTTP).
/// </summary>
public sealed class EsquemaHealthCheck(ISqlConnectionFactory conexiones) : IHealthCheck
{
    private static readonly string[] ProcedimientosEsperados =
    [
        "SP_CORTE_ResolverInicio", "SP_CORTE_Crear",
        "SP_Pendientes_CALIMACO_Ingreso", "SP_Pendientes_CALIMACO_Salida",
        "SP_Pendientes_CMP_Ingreso", "SP_Pendientes_CMP_Salida",
        "SP_Pendientes_SICA_Ingreso", "SP_Pendientes_SICA_Salida",
    ];

    private static readonly string[] TablasEsperadas = ["Corte", "bitacora_transacciones"];

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await using var conexion = await conexiones.AbrirAsync(ct);

            var procedimientosFaltantes = await FaltantesAsync(conexion, "P", ProcedimientosEsperados, ct);
            if (procedimientosFaltantes.Count > 0)
                return HealthCheckResult.Unhealthy($"Faltan Stored Procedures en bd_autobot: {string.Join(", ", procedimientosFaltantes)}");

            var tablasFaltantes = await FaltantesAsync(conexion, "U", TablasEsperadas, ct);
            if (tablasFaltantes.Count > 0)
                return HealthCheckResult.Unhealthy($"Faltan tablas en bd_autobot: {string.Join(", ", tablasFaltantes)}");

            return HealthCheckResult.Healthy();
        }
        catch (SqlException ex)
        {
            return HealthCheckResult.Unhealthy("No se pudo verificar el esquema de bd_autobot", ex);
        }
    }

    private static async Task<List<string>> FaltantesAsync(SqlConnection conexion, string tipo, IReadOnlyCollection<string> nombresEsperados, CancellationToken ct)
    {
        await using var comando = new SqlCommand(
            "SELECT name FROM sys.objects WHERE type = @tipo AND name IN (SELECT value FROM STRING_SPLIT(@nombres, ','))",
            conexion);
        comando.Parameters.AddWithValue("tipo", tipo);
        comando.Parameters.AddWithValue("nombres", string.Join(',', nombresEsperados));

        var encontrados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var lector = await comando.ExecuteReaderAsync(ct);
        while (await lector.ReadAsync(ct))
            encontrados.Add(lector.GetString(0));

        return nombresEsperados.Where(n => !encontrados.Contains(n)).ToList();
    }
}
