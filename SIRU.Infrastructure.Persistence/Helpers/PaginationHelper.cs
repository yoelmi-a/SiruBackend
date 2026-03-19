using SIRU.Core.Domain.Common.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Infrastructure.Persistence.Helpers
{
    public static class PaginationHelper
    {
        public static async Task<PaginatedResponse<T>> PaginateAsync<T>(this IQueryable<T> source, Pagination pagination)
        {

            var res = await source.Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize).ToArrayAsync();
            return new PaginatedResponse<T>
            {
                Items = res,
                Pagination = pagination
            };
        }
    }
}
