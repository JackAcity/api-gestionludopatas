using Microsoft.Data.SqlClient;

namespace GestionLudopatas.Api.Infrastructure.Sql;

/// <summary>
/// Puerto para obtener conexiones a <c>bd_autobot</c>. Fase 1: la connection string sale
/// de <see cref="IConfiguration"/> poblada por `appsettings`/variables de entorno planas.
/// Fase 2 (D8): la misma interfaz, pero <see cref="IConfiguration"/> queda poblada por
/// el fetch a Vault hecho en <c>Program.cs</c> antes de <c>builder.Build()</c> — este tipo
/// no cambia entre fases.
/// </summary>
public interface ISqlConnectionFactory
{
    Task<SqlConnection> AbrirAsync(CancellationToken ct);
}

public sealed class SqlConnectionFactory(IConfiguration configuracion) : ISqlConnectionFactory
{
    public async Task<SqlConnection> AbrirAsync(CancellationToken ct)
    {
        var cadenaConexion = configuracion.GetConnectionString("BdAutobot");
        if (string.IsNullOrEmpty(cadenaConexion))
            throw new InvalidOperationException("ConnectionStrings:BdAutobot no está configurada.");

        var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(ct);
        return conexion;
    }
}
