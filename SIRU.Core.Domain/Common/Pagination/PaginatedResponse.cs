using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Domain.Common.Pagination
{
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public Pagination Pagination { get; set; }

        
        public PaginatedResponse<TMap> Map<TMap>(Func<T, TMap> mapFunc) where TMap : class
        {
            var mappedItems = Items.Select(mapFunc);
            return new PaginatedResponse<TMap>
            {
                Items = mappedItems,
                Pagination = Pagination
            };
        }
    }
}
