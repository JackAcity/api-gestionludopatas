using GestionLudopatas.Api.Idempotencia;
using GestionLudopatas.Api.Infrastructure.Idempotencia;
using GestionLudopatas.Api.Infrastructure.Sql;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GestionLudopatas.Api.Tests.Infrastructure.Sql;

public class PersistenciaExtensionesTests
{
    [Fact]
    public void Registra_el_puerto_de_idempotencia_con_su_adaptador_en_memoria()
    {
        var servicios = new ServiceCollection();
        servicios.AddPersistenciaSql();

        using var proveedor = servicios.BuildServiceProvider();

        Assert.IsType<IdempotencyStore>(proveedor.GetRequiredService<IIdempotencyStore>());
    }
}
