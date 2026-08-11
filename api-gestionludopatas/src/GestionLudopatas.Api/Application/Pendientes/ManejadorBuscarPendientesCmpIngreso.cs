using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Application.Pendientes;

/// <summary>Caso de uso de <c>SP_Pendientes_CMP_Ingreso</c> (spec pendientes-cmp-ingreso).</summary>
public sealed class ManejadorBuscarPendientesCmpIngreso(IBuscarPendientesCmpIngreso puerto)
    : ManejadorBuscarPendientesCalimacoCmpBase<PendienteCmpItem, IBuscarPendientesCmpIngreso>(puerto)
{
    protected override string CodigoCorteInvalido => CodigoError.PendCmpIngresoCorteInvalido;
    protected override string CodigoMaxReintentosInvalido => CodigoError.PendCmpIngresoMaxReintentosInvalido;
    protected override string CodigoReintentoForzadoInvalido => CodigoError.PendCmpIngresoReintentoForzadoInvalido;
}
