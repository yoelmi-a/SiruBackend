using Microsoft.EntityFrameworkCore;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infrastructure.Persistence.Repositories
{
    public class PositionRepository : GenericRepository<Position>, IPositionRepository
    {
        public PositionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Position?> GetByIdWithDepartmentAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
