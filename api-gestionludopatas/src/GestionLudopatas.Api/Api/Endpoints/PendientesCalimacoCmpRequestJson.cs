using System.Text.Json;
using GestionLudopatas.Api.Application.Pendientes;
using GestionLudopatas.Api.Application.Resultados;
using GestionLudopatas.Api.Domain.Errores;

namespace GestionLudopatas.Api.Endpoints;

/// <summary>
/// Convierte el cuerpo HTTP de los cuatro endpoints CALIMACO/CMP en un request de
/// Application. Es explícito porque el contrato diferencia una propiedad booleana
/// omitida de una enviada como <c>null</c>; el binding estándar a <c>bool?</c> no lo hace.
/// Los tipos JSON incompatibles se clasifican aquí como 400 GL-API-REQ-001, antes de la
/// validación funcional 422 del manejador.
/// </summary>
public static class PendientesCalimacoCmpRequestJson
{
    public static Result<PendientesCalimacoCmpRequest> DesdeJson(JsonElement cuerpo)
    {
        if (cuerpo.ValueKind is not JsonValueKind.Object)
            return SolicitudInvalida();

        if (!TryLeerEnteroNullable(cuerpo, "corteIdActual", out var corteIdActual)
            || !TryLeerEnteroNullable(cuerpo, "maxReintentosPorSistema", out var maxReintentos)
            || !TryLeerBooleanoNullable(cuerpo, "esReintentoForzado", out var esReintentoForzado, out var esReintentoForzadoEnviado))
            return SolicitudInvalida();

        return Result<PendientesCalimacoCmpRequest>.Ok(
            new PendientesCalimacoCmpRequest(corteIdActual, maxReintentos, esReintentoForzado, esReintentoForzadoEnviado));
    }

    private static bool TryLeerEnteroNullable(JsonElement cuerpo, string nombre, out int? valor)
    {
        valor = null;
        if (!cuerpo.TryGetProperty(nombre, out var propiedad) || propiedad.ValueKind is JsonValueKind.Null)
            return true;

        if (propiedad.ValueKind is not JsonValueKind.Number || !propiedad.TryGetInt32(out var entero))
            return false;

        valor = entero;
        return true;
    }

    private static bool TryLeerBooleanoNullable(JsonElement cuerpo, string nombre, out bool? valor, out bool enviado)
    {
        valor = null;
        enviado = cuerpo.TryGetProperty(nombre, out var propiedad);
        if (!enviado || propiedad.ValueKind is JsonValueKind.Null)
            return true;

        if (propiedad.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            return false;

        valor = propiedad.GetBoolean();
        return true;
    }

    private static Result<PendientesCalimacoCmpRequest> SolicitudInvalida() =>
        Result<PendientesCalimacoCmpRequest>.Fallo(
            StatusCodes.Status400BadRequest,
            CodigoError.ApiRequestInvalido,
            "El cuerpo JSON debe ser un objeto con propiedades de los tipos esperados.");
}
