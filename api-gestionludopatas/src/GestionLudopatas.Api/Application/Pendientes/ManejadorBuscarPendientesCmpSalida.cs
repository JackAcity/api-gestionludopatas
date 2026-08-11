using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Application.Pendientes;

/// <summary>Caso de uso de <c>SP_Pendientes_CMP_Salida</c> (spec pendientes-cmp-salida).</summary>
public sealed class ManejadorBuscarPendientesCmpSalida(IBuscarPendientesCmpSalida puerto)
    : ManejadorBuscarPendientesCalimacoCmpBase<PendienteCmpItem, IBuscarPendientesCmpSalida>(puerto)
{
    protected override string CodigoCorteInvalido => CodigoError.PendCmpSalidaCorteInvalido;
    protected override string CodigoMaxReintentosInvalido => CodigoError.PendCmpSalidaMaxReintentosInvalido;
    protected override string CodigoReintentoForzadoInvalido => CodigoError.PendCmpSalidaReintentoForzadoInvalido;
}
