using GestionLudopatas.Api.Application.Pendientes;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>
/// Adaptador de <c>dbo.SP_Pendientes_SICA_Ingreso</c> (spec pendientes-sica-ingreso).
/// <c>nombresApellidos</c> se lee como no-nulo — el SP ya filtra <c>IS NOT NULL</c>.
/// </summary>
public sealed class SicaIngresoBuscadorSql(ISqlConnectionFactory conexiones) : SicaBuscadorSqlBase<PendienteSicaIngresoItem>(conexiones)
{
    protected override string NombreSp => "dbo.SP_Pendientes_SICA_Ingreso";

    protected override PendienteSicaIngresoItem MapearFila(SqlDataReader lector) => new(
        lector.GetInt32(lector.GetOrdinal("id")),
        lector.GetString(lector.GetOrdinal("numero_documento")),
        lector.GetString(lector.GetOrdinal("tipo_documento")),
        lector.GetString(lector.GetOrdinal("nombres_apellidos")),
        LeerNullableString(lector, "fecha_inscripcion"),
        lector.GetInt32(lector.GetOrdinal("corte_id")),
        lector.GetInt32(lector.GetOrdinal("ultimo_corte_id")),
        lector.GetInt32(lector.GetOrdinal("sica_reintentos")));
}
