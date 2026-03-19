using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Domain.Common.Pagination
{
    public class Pagination
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Pagination()
        {
        }
        public Pagination(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize < 1 ? 10 : pageSize;
        }
    }
}
