using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Application.Pendientes;

/// <summary>Caso de uso de <c>SP_Pendientes_SICA_Salida</c> (spec pendientes-sica-salida).</summary>
public sealed class ManejadorBuscarPendientesSicaSalida(IBuscarPendientes<PendientesSicaRequest, PendienteSicaSalidaItem> puerto)
    : ManejadorBuscarPendientesSicaBase<PendienteSicaSalidaItem>(puerto)
{
    protected override string CodigoMaxReintentosInvalido => CodigoError.PendSicaSalidaMaxReintentosInvalido;
}
