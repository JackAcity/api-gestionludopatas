using System.Data;
using GestionLudopatas.Api.Application.Cortes;
using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>Adaptador de <see cref="ICorteCreator"/> contra <c>dbo.SP_CORTE_Crear</c>. Sin validación de negocio — eso vive en <see cref="ManejadorCrearCorte"/> (spec casos-uso-result-negocio).</summary>
public sealed class CorteCreatorSql(ISqlConnectionFactory conexiones) : ICorteCreator
{
    public async Task<CrearCorteResponse> CrearAsync(CrearCorteRequest request, CancellationToken ct)
    {
        await using var conexion = await conexiones.AbrirAsync(ct);
        await using var comando = new SqlCommand("dbo.SP_CORTE_Crear", conexion) { CommandType = CommandType.StoredProcedure };

        comando.Parameters.AddWithValue("@TipoCorte", request.TipoCorte!);
        comando.Parameters.Add("@FechaHoraCorte", SqlDbType.DateTime).Value =
            request.FechaHoraCorte is { } fecha ? fecha.UtcDateTime : DBNull.Value;
        comando.Parameters.AddWithValue("@FechaHoraEjecucion", request.FechaHoraEjecucion!.Value.UtcDateTime);

        var salidaCorteId = comando.Parameters.Add("@corte_id", SqlDbType.Int);
        salidaCorteId.Direction = ParameterDirection.Output;

        await comando.ExecuteNonQueryAsync(ct);

        return new CrearCorteResponse((int)salidaCorteId.Value);
    }
}
