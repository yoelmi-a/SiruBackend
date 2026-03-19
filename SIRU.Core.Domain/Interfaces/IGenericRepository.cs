using SIRU.Core.Domain.Common.Pagination;
using System.Linq.Expressions;

namespace SIRU.Core.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync<TKey>(TKey id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<PaginatedResponse<T>> Paginate(Pagination pagination);
        Task<PaginatedResponse<T>> PaginateWhere(Pagination pagination, Expression<Func<T, bool>> predicate);
        Task<T> UpdateAsync(T entity);
        Task RemoveAsync(T entity);
    }
}
