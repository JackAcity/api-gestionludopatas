using System.Data;
using GestionLudopatas.Api.Application.Pendientes;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>
/// Base común a los 4 SP de pendientes CALIMACO/CMP (D4) — mismo request, misma firma de
/// parámetros (<c>@corte_id_actual</c>, <c>@MaxReintentosPorSistema</c>, <c>@in_EsReintentoForzado</c>),
/// solo difiere el SP a ejecutar y el mapeo de fila. Sin validación de negocio — eso vive
/// en <c>ManejadorBuscarPendientesCalimacoCmpBase</c> (Application, spec casos-uso-result-negocio).
/// </summary>
public abstract class CalimacoCmpBuscadorSqlBase<TItem>(ISqlConnectionFactory conexiones)
    : IBuscarPendientes<PendientesCalimacoCmpRequest, TItem>
{
    protected abstract string NombreSp { get; }
    protected abstract TItem MapearFila(SqlDataReader lector);

    public async Task<IReadOnlyList<TItem>> BuscarAsync(PendientesCalimacoCmpRequest request, CancellationToken ct)
    {
        await using var conexion = await conexiones.AbrirAsync(ct);
        await using var comando = new SqlCommand(NombreSp, conexion) { CommandType = CommandType.StoredProcedure };
        comando.Parameters.AddWithValue("@corte_id_actual", request.CorteIdActual!.Value);
        comando.Parameters.AddWithValue("@MaxReintentosPorSistema", request.MaxReintentosPorSistema!.Value);
        comando.Parameters.AddWithValue("@in_EsReintentoForzado", request.EsReintentoForzado ?? false);

        var resultado = new List<TItem>();
        await using var lector = await comando.ExecuteReaderAsync(ct);
        while (await lector.ReadAsync(ct))
            resultado.Add(MapearFila(lector));
        return resultado;
    }

    protected static int? LeerNullableInt(SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetInt32(indice);
    }
}
