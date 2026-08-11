using GestionLudopatas.Api.Application.Cortes;
using GestionLudopatas.Api.Application.Pendientes;
using GestionLudopatas.Api.Idempotencia;
using GestionLudopatas.Api.Infrastructure.Idempotencia;

namespace GestionLudopatas.Api.Infrastructure.Sql;

public static class PersistenciaExtensiones
{
    public static IServiceCollection AddPersistenciaSql(this IServiceCollection servicios)
    {
        servicios.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        servicios.AddScoped<ICorteResolver, CorteResolverSql>();
        servicios.AddScoped<ICorteCreator, CorteCreatorSql>();
        servicios.AddScoped<ManejadorResolverInicioCorte>();
        servicios.AddScoped<ManejadorCrearCorte>();

        // Puertos de pendientes CALIMACO/CMP registrados por su interfaz marcador
        // (no por IBuscarPendientes<TRequest,TItem> genérico): ingreso y salida
        // comparten el mismo TRequest/TItem — el marcador es lo único que distingue
        // "cuál adaptador" para el contenedor de DI (ver IBuscarPendientes.cs).
        servicios.AddScoped<IBuscarPendientesCalimacoIngreso, CalimacoIngresoBuscadorSql>();
        servicios.AddScoped<IBuscarPendientesCalimacoSalida, CalimacoSalidaBuscadorSql>();
        servicios.AddScoped<IBuscarPendientesCmpIngreso, CmpIngresoBuscadorSql>();
        servicios.AddScoped<IBuscarPendientesCmpSalida, CmpSalidaBuscadorSql>();
        servicios.AddScoped<ManejadorBuscarPendientesCalimacoIngreso>();
        servicios.AddScoped<ManejadorBuscarPendientesCalimacoSalida>();
        servicios.AddScoped<ManejadorBuscarPendientesCmpIngreso>();
        servicios.AddScoped<ManejadorBuscarPendientesCmpSalida>();

        // SICA ingreso/salida sí tienen TItem distinto — el genérico ya es unívoco.
        servicios.AddScoped<IBuscarPendientes<PendientesSicaRequest, PendienteSicaIngresoItem>, SicaIngresoBuscadorSql>();
        servicios.AddScoped<IBuscarPendientes<PendientesSicaRequest, PendienteSicaSalidaItem>, SicaSalidaBuscadorSql>();
        servicios.AddScoped<ManejadorBuscarPendientesSicaIngreso>();
        servicios.AddScoped<ManejadorBuscarPendientesSicaSalida>();

        servicios.AddHealthChecks().AddCheck<EsquemaHealthCheck>("bd_autobot_esquema");

        servicios.AddSingleton<IIdempotencyStore, IdempotencyStore>();

        return servicios;
    }
}
