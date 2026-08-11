using GestionLudopatas.Api.Application.Errores;

namespace GestionLudopatas.Api.Application.Resultados;

/// <summary>
/// Fallo de negocio esperado (spec casos-uso-result-negocio), con los datos estables que
/// necesita el adaptador HTTP para construir el contrato de error.
/// </summary>
/// <summary>
/// Resultado de un caso de uso: éxito con <see cref="Value"/>, o fallo esperado con
/// <see cref="Error"/>. Un caso de uso que encuentra una regla de negocio incumplida
/// devuelve <see cref="Fallo(ResultadoError)"/> — nunca lanza para ese camino (spec
/// casos-uso-result-negocio).
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly ResultadoError? _error;

    public bool IsSuccess { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Result es un fallo — no tiene Value. Revisar IsSuccess/Error antes de acceder.");

    public ResultadoError Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException("Result es un éxito — no tiene Error. Revisar IsSuccess/Value antes de acceder.");

    private Result(bool isSuccess, T? value, ResultadoError? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);

    public static Result<T> Fallo(ResultadoError error) => new(false, default, error);

    public static Result<T> Fallo(int status, string codigo, string detalle, bool reintentable = false, string origen = "api", int? sqlErrorNumber = null) =>
        Fallo(new ResultadoError(status, codigo, detalle, reintentable, origen, sqlErrorNumber));
}
