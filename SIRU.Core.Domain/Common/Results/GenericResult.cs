using SIRU.Core.Domain.Common.Enums;

namespace SIRU.Core.Domain.Common.Results;

/// <summary>
/// Result genérico que encapsula tanto el éxito/fallo como un valor de retorno.
/// Hereda de Result base para mantener las propiedades IsSuccess, IsFailure y Error.
/// 
/// CUÁNDO USAR:
/// - Operaciones que retornan un valor: ObtenerUsuario(), CalcularDescuento(), etc.
/// - Conversiones: ConvertirAEntero(), ParsearFecha(), etc.
/// - Búsquedas: BuscarPorId(), EncontrarPrimero(), etc.
/// </summary>
/// <typeparam name="T">Tipo del valor que se retorna en caso de éxito</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Valor retornado en caso de éxito.
    /// IMPORTANTE: Solo acceder a este valor después de verificar IsSuccess = true
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Constructor interno para forzar el uso de factory methods
    /// </summary>
    /// <param name="value">Valor a almacenar</param>
    /// <param name="isSuccess">Estado de la operación</param>
    /// <param name="errors">Errores en caso de fallos</param>
    internal Result(bool isSuccess, ICollection<string>? errors, T? value = default, ErrorType? errorTypeCode = null) : base(isSuccess, errors, errorTypeCode)
    {
        Value = value;
    }
}