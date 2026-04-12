namespace SIRU.Core.Domain.Common
{
    /// <summary>
    /// Clase que encapsula los parámetros de paginación que vienen de la URL o formulario
    /// Se usa para validar y normalizar los valores de entrada
    /// </summary>
    public class PaginationParameters
    {
        // Constante que define el máximo de elementos por página permitidos
        // Previene que alguien solicite 10,000 elementos por página y afecte el rendimiento
        private const int MaxPageSize = 50;

        // Campo privado para controlar el valor de PageSize
        private int _pageSize = 10; // Valor por defecto: 10 elementos por página

        /// <summary>
        /// Número de página solicitada
        /// Valor por defecto: 1 (primera página)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Cantidad de elementos por página con validación automática
        /// Si se intenta asignar un valor mayor a MaxPageSize, se limita automáticamente
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
            // Si alguien solicita 100 elementos por página, se limita a 50
        }
    }
}
