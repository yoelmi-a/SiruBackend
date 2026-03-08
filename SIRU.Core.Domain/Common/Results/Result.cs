namespace SIRU.Core.Domain.Common.Results
{
    /// <summary>
    /// Clase base Result para operaciones que pueden fallar sin retornar un valor específico.
    /// Ejemplo: operaciones de guardado, envío de emails, validaciones simples.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indica si la operación fue exitosa
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Mensajes de error cuando la operación falla
        /// </summary>
        public ICollection<string> Error { get; }

        /// <summary>
        /// Constructor protegido para forzar el uso de factory methods
        /// Esto garantiza que solo se puedan crear Results válidos
        /// </summary>
        /// <param name="isSuccess">True si la operación fue exitosa</param>
        /// <param name="errors">Mensajes de error (solo requerido cuando isSuccess = false)</param>
        /// <exception cref="InvalidOperationException">
        /// - Si isSuccess=true, pero hay error (estado inconsistente)
        /// - Si isSuccess=false, pero no hay error (fallo sin descripción)
        /// </exception>
        protected Result(bool isSuccess, ICollection<string>? errors = null)
        {
            var errorsList = errors ?? [];
            // Invariante 1: Un éxito no puede tener error
            if (isSuccess && errorsList.Count > 0)
            {
                throw new InvalidOperationException("Success result cannot have error");
            }

            // Invariante 2: Un fallo debe tener descripción del error
            if (!isSuccess && errorsList.Count == 0)
            {
                throw new InvalidOperationException("Failure result must have error");
            }

            IsSuccess = isSuccess;
            Error = errorsList;
        }

        /// <summary>
        /// Factory method para crear un Result exitoso sin valor de retorno
        /// Uso típico: Result.Success() para operaciones como "GuardarUsuario()"
        /// </summary>
        public static Result Success() => new Result(true);

        /// <summary>
        /// Factory method para crear un Result fallido sin valor de retorno
        /// </summary>
        /// <param name="errors">Lista con los errores ocurridos</param>
        public static Result Failure(ICollection<string> errors) => new Result(false, errors);

        /// <summary>
        /// Factory method para crear un Result exitoso con valor de retorno
        /// Uso típico: Result.Success(usuario) para operaciones como "ObtenerUsuario()"
        /// </summary>
        /// <typeparam name="T">Tipo del valor de retorno</typeparam>
        /// <param name="value">Valor a retornar en caso de éxito</param>
        public static Result<T> Success<T>(T value) => new Result<T>(true, null, value);

        /// <summary>
        /// Factory method para crear un Result fallido con tipo específico
        /// </summary>
        /// <typeparam name="T">Tipo que debería retornar en caso de éxito</typeparam>
        /// <param name="errors">Lista de errores</param>
        public static Result<T> Failure<T>(List<string> errors) => new Result<T>(false, errors);
    }
}
