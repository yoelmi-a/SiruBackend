using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;
using System.Linq.Expressions;

namespace SIRU.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        public GenericRepository(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _context.Set<TEntity>().AnyAsync(filter);
        }

        public virtual async Task<Result<TEntity>> UpdateAsync(TEntity entity, TKey id)
        {
            var entry = await _context.Set<TEntity>().FindAsync(id);

            if (entry != null)
            {
                _context.Entry(entry).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
                return Result.Success(entity);
            }

            return Result.Failure<TEntity>(_config[$"{nameof(TEntity)}Errors:NotFound"] ?? "");
        }

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<ICollection<TEntity>> GetAllAsync()
        {
            var entities = await _context.Set<TEntity>().ToListAsync();
            return entities;
        }

        public async Task<ICollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter)
        {
            var entities = await _context.Set<TEntity>().Where(filter).ToListAsync();
            return entities;
        }

        public virtual async Task<PagedResult<TEntity>> GetAllListAsync(PaginationParameters parameters)
        {
            var totalCount = await _context.Set<TEntity>().CountAsync();

            var entities = await _context.Set<TEntity>()
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<TEntity>(entities, parameters.Page, parameters.PageSize, totalCount);
        }

        public virtual async Task<PagedResult<TEntity>> GetAllListAsync(PaginationParameters parameters, Expression<Func<TEntity, bool>> filter)
        {
            var totalCount = await _context.Set<TEntity>().Where(filter).CountAsync();

            var entities = await _context.Set<TEntity>()
                .Where(filter)
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<TEntity>(entities, parameters.Page, parameters.PageSize, totalCount);
        }
    }
}
