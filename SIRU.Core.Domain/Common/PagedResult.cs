namespace SIRU.Core.Domain.Common
{
    /// <summary>
    /// Clase genérica que encapsula los resultados paginados
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>
        /// Lista de elementos de la página actual
        /// Solo contiene los elementos que se mostrarán en esta página específica
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Número de página actual (empezando desde 1)
        /// Ejemplo: Si estamos en la página 3, este valor será 3
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Cantidad de elementos por página
        /// Ejemplo: Si mostramos 10 elementos por página, este valor será 10
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total de elementos en toda la base de datos (sin paginar)
        /// Ejemplo: Si hay 1000 clientes en total, este valor será 1000
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total de páginas calculado automáticamente
        /// Se calcula dividiendo TotalCount entre PageSize y redondeando hacia arriba
        /// Ejemplo: 1000 elementos ÷ 10 por página = 100 páginas
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Propiedad calculada que indica si existe una página anterior
        /// Útil para mostrar/ocultar el botón "Anterior" en la UI
        /// </summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Propiedad calculada que indica si existe una página siguiente
        /// Útil para mostrar/ocultar el botón "Siguiente" en la UI
        /// </summary>
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// Constructor vacío para inicialización
        /// </summary>
        public PagedResult()
        {
            Items = new List<T>();
        }

        /// <summary>
        /// Constructor principal que calcula automáticamente TotalPages
        /// </summary>
        /// <param name="items">Lista de elementos de la página actual</param>
        /// <param name="page">Número de página actual</param>
        /// <param name="pageSize">Elementos por página</param>
        /// <param name="totalCount">Total de elementos en la BD</param>
        public PagedResult(List<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;

            // Cálculo automático del total de páginas
            // Math.Ceiling redondea hacia arriba: 101 elementos ÷ 10 = 10.1 → 11 páginas
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}