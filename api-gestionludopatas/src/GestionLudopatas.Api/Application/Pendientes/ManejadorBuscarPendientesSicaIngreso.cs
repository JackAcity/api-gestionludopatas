using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Application.Pendientes;

/// <summary>Caso de uso de <c>SP_Pendientes_SICA_Ingreso</c> (spec pendientes-sica-ingreso).</summary>
public sealed class ManejadorBuscarPendientesSicaIngreso(IBuscarPendientes<PendientesSicaRequest, PendienteSicaIngresoItem> puerto)
    : ManejadorBuscarPendientesSicaBase<PendienteSicaIngresoItem>(puerto)
{
    protected override string CodigoMaxReintentosInvalido => CodigoError.PendSicaIngresoMaxReintentosInvalido;
}
