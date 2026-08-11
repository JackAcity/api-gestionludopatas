using GestionLudopatas.Api.Application.Pendientes;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>Adaptador de <c>dbo.SP_Pendientes_CMP_Ingreso</c> (spec pendientes-cmp-ingreso).</summary>
public sealed class CmpIngresoBuscadorSql(ISqlConnectionFactory conexiones) : CalimacoCmpBuscadorSqlBase<PendienteCmpItem>(conexiones), IBuscarPendientesCmpIngreso
{
    protected override string NombreSp => "dbo.SP_Pendientes_CMP_Ingreso";

    protected override PendienteCmpItem MapearFila(SqlDataReader lector) => new(
        lector.GetInt32(lector.GetOrdinal("id")),
        lector.GetString(lector.GetOrdinal("numero_documento")),
        lector.GetString(lector.GetOrdinal("tipo_documento")),
        lector.GetInt32(lector.GetOrdinal("corte_id")),
        lector.GetInt32(lector.GetOrdinal("ultimo_corte_id")),
        LeerNullableInt(lector, "cmp_ultimo_corte_id"),
        lector.GetInt32(lector.GetOrdinal("cmp_reintentos")));
}
