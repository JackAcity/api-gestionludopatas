using GestionLudopatas.Api.Application.Pendientes;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>Adaptador de <c>dbo.SP_Pendientes_CALIMACO_Ingreso</c> (spec pendientes-calimaco-ingreso).</summary>
public sealed class CalimacoIngresoBuscadorSql(ISqlConnectionFactory conexiones) : CalimacoCmpBuscadorSqlBase<PendienteCalimacoItem>(conexiones), IBuscarPendientesCalimacoIngreso
{
    protected override string NombreSp => "dbo.SP_Pendientes_CALIMACO_Ingreso";

    protected override PendienteCalimacoItem MapearFila(SqlDataReader lector) => new(
        lector.GetInt32(lector.GetOrdinal("id")),
        lector.GetString(lector.GetOrdinal("numero_documento")),
        lector.GetString(lector.GetOrdinal("tipo_documento")),
        lector.GetInt32(lector.GetOrdinal("corte_id")),
        lector.GetInt32(lector.GetOrdinal("ultimo_corte_id")),
        LeerNullableInt(lector, "calimaco_ultimo_corte_id"),
        lector.GetInt32(lector.GetOrdinal("calimaco_reintentos")));
}
