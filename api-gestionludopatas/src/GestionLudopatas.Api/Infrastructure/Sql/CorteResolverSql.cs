using GestionLudopatas.Api.Application.Cortes;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>
/// Adaptador de <see cref="ICorteResolver"/> contra <c>dbo.SP_CORTE_ResolverInicio</c>. Sin
/// validación de negocio — eso vive en <see cref="ManejadorResolverInicioCorte"/> (spec
/// casos-uso-result-negocio). Conexión y transacción propias (no compartidas con otros
/// adaptadores) — el SP hace su propio bloqueo/actualización condicional (UPDLOCK,
/// HOLDLOCK) internamente; la transacción del caller garantiza que ese trabajo se
/// confirma o revierte como unidad.
/// </summary>
public sealed class CorteResolverSql(ISqlConnectionFactory conexiones) : ICorteResolver
{
    public async Task<ResolverInicioResponse> ResolverAsync(ResolverInicioRequest request, CancellationToken ct)
    {
        await using var conexion = await conexiones.AbrirAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);

        await using var comando = new SqlCommand("dbo.SP_CORTE_ResolverInicio", conexion, (SqlTransaction)transaccion)
        {
            CommandType = System.Data.CommandType.StoredProcedure,
        };
        comando.Parameters.AddWithValue("@FechaHoraEvaluacion", request.FechaHoraEvaluacion!.Value.UtcDateTime);
        comando.Parameters.AddWithValue("@TimeoutMinutos", request.TimeoutMinutos!.Value);

        await using var lector = await comando.ExecuteReaderAsync(ct);
        if (!await lector.ReadAsync(ct))
            throw new InvalidOperationException("SP_CORTE_ResolverInicio no devolvió ninguna fila.");

        var respuesta = new ResolverInicioResponse(
            Accion: lector.GetString(lector.GetOrdinal("accion")),
            CorteId: LeerNullableInt(lector, "corte_id"),
            CorteColgadoOficialId: LeerNullableInt(lector, "corte_colgado_oficial_id"),
            CorteColgadoManualId: LeerNullableInt(lector, "corte_colgado_manual_id"));

        await lector.CloseAsync();
        await transaccion.CommitAsync(ct);
        return respuesta;
    }

    private static int? LeerNullableInt(SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetInt32(indice);
    }
}
