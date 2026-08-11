using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Application.Pendientes;

/// <summary>Caso de uso de <c>SP_Pendientes_CALIMACO_Ingreso</c> (spec pendientes-calimaco-ingreso).</summary>
public sealed class ManejadorBuscarPendientesCalimacoIngreso(IBuscarPendientesCalimacoIngreso puerto)
    : ManejadorBuscarPendientesCalimacoCmpBase<PendienteCalimacoItem, IBuscarPendientesCalimacoIngreso>(puerto)
{
    protected override string CodigoCorteInvalido => CodigoError.PendCalimacoIngresoCorteInvalido;
    protected override string CodigoMaxReintentosInvalido => CodigoError.PendCalimacoIngresoMaxReintentosInvalido;
    protected override string CodigoReintentoForzadoInvalido => CodigoError.PendCalimacoIngresoReintentoForzadoInvalido;
}
