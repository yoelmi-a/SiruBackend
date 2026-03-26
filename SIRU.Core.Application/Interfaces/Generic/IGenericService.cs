using SIRU.Core.Application.Dtos.Vacant;
using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Application.Interfaces.Generic
{
    public interface IGenericService<T, TDto, TKey> where T : BaseEntity<TKey>where TDto : class
    {
        Task<Result<IEnumerable<TDto>>> GetAllAsync();

        Task<Result<TDto>> GetByIdAsync(TKey id);

        Task<Result<TDto>> AddAsync(TDto dto);

        Task<Result> UpdateAsync(TKey id, TDto dto);

        Task<Result> DeleteAsync(TKey id);
    }
}