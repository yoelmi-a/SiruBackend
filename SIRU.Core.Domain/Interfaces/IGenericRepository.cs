using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Common.Results;
using System.Linq.Expressions;

namespace SIRU.Core.Domain.Interfaces
{
    public interface IGenericRepository<TEntity, TKey>
        where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity entity);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter);
        Task<Result<TEntity>> UpdateAsync(TEntity entity, TKey id);
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<ICollection<TEntity>> GetAllAsync();
        Task<ICollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter);
        Task<PagedResult<TEntity>> GetAllListAsync(PaginationParameters parameters);
        Task<PagedResult<TEntity>> GetAllListAsync(PaginationParameters parameters, Expression<Func<TEntity, bool>> filter);
        Task SaveChangesAsync();
    }
}
