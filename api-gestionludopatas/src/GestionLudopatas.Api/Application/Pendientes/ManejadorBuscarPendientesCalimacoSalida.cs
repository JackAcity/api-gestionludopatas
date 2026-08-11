using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Application.Pendientes;

/// <summary>Caso de uso de <c>SP_Pendientes_CALIMACO_Salida</c> (spec pendientes-calimaco-salida).</summary>
public sealed class ManejadorBuscarPendientesCalimacoSalida(IBuscarPendientesCalimacoSalida puerto)
    : ManejadorBuscarPendientesCalimacoCmpBase<PendienteCalimacoItem, IBuscarPendientesCalimacoSalida>(puerto)
{
    protected override string CodigoCorteInvalido => CodigoError.PendCalimacoSalidaCorteInvalido;
    protected override string CodigoMaxReintentosInvalido => CodigoError.PendCalimacoSalidaMaxReintentosInvalido;
    protected override string CodigoReintentoForzadoInvalido => CodigoError.PendCalimacoSalidaReintentoForzadoInvalido;
}
