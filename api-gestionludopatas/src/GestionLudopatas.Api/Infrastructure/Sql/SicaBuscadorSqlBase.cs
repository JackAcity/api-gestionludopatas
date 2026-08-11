using System.Data;
using GestionLudopatas.Api.Application.Pendientes;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>
/// Base común a los 2 SP de pendientes SICA (D4) — SICA no usa <c>corteIdActual</c> ni
/// <c>esReintentoForzado</c>, solo <c>@MaxReintentosPorSistema</c>. Sin validación de
/// negocio — eso vive en <c>ManejadorBuscarPendientesSicaBase</c> (Application, spec
/// casos-uso-result-negocio).
/// </summary>
public abstract class SicaBuscadorSqlBase<TItem>(ISqlConnectionFactory conexiones)
    : IBuscarPendientes<PendientesSicaRequest, TItem>
{
    protected abstract string NombreSp { get; }
    protected abstract TItem MapearFila(SqlDataReader lector);

    public async Task<IReadOnlyList<TItem>> BuscarAsync(PendientesSicaRequest request, CancellationToken ct)
    {
        await using var conexion = await conexiones.AbrirAsync(ct);
        await using var comando = new SqlCommand(NombreSp, conexion) { CommandType = CommandType.StoredProcedure };
        comando.Parameters.AddWithValue("@MaxReintentosPorSistema", request.MaxReintentosPorSistema!.Value);

        var resultado = new List<TItem>();
        await using var lector = await comando.ExecuteReaderAsync(ct);
        while (await lector.ReadAsync(ct))
            resultado.Add(MapearFila(lector));
        return resultado;
    }

    protected static string? LeerNullableString(SqlDataReader lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetString(indice);
    }
}
