using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Application.Interfaces.Common
{
    public interface IServiceBase<TEntity,TID,TDto, TInsertDto,TUpdateDto> where TEntity : class 
        where TDto : class
        where TInsertDto : class
        where TUpdateDto : class
    {
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<Result<TDto>> GetByIdAsync(TID id);
        Task<Result<TDto>> AddAsync(TInsertDto dto);
        Task<Result<TDto>> UpdateAsync(TID id, TUpdateDto dto);
        Task<Result> DeleteAsync(TID id);
        Task<PaginatedResponse<TDto>> Paginate(Pagination pagination);
    }
}
