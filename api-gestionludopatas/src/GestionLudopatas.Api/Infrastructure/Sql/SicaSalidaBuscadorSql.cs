using GestionLudopatas.Api.Application.Pendientes;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>
/// Adaptador de <c>dbo.SP_Pendientes_SICA_Salida</c> (spec pendientes-sica-salida).
/// A diferencia de ingreso, <c>nombresApellidos</c> y <c>fechaInscripcion</c> pueden ser nulos.
/// </summary>
public sealed class SicaSalidaBuscadorSql(ISqlConnectionFactory conexiones) : SicaBuscadorSqlBase<PendienteSicaSalidaItem>(conexiones)
{
    protected override string NombreSp => "dbo.SP_Pendientes_SICA_Salida";

    protected override PendienteSicaSalidaItem MapearFila(SqlDataReader lector) => new(
        lector.GetInt32(lector.GetOrdinal("id")),
        lector.GetString(lector.GetOrdinal("numero_documento")),
        lector.GetString(lector.GetOrdinal("tipo_documento")),
        LeerNullableString(lector, "nombres_apellidos"),
        LeerNullableString(lector, "fecha_inscripcion"),
        lector.GetInt32(lector.GetOrdinal("corte_id")),
        lector.GetInt32(lector.GetOrdinal("ultimo_corte_id")),
        lector.GetInt32(lector.GetOrdinal("sica_reintentos")));
}
