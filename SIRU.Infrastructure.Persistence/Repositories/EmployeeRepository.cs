using Microsoft.EntityFrameworkCore;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infrastructure.Persistence.Repositories;

/// <summary>
/// Specific repository for Employee with work history queries.
/// </summary>
public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EmployeePosition>> GetEmployeeHistoryWithDetailsAsync(string employeeId)
    {
        return await _dbSet
            .Where(e => e.Id == employeeId)
            .SelectMany(e => e.PositionsOccupied!)
            .Include(ep => ep.Position!)
                .ThenInclude(p => p.Department!)
            .OrderByDescending(ep => ep.StartDate)
            .ToListAsync();
    }
}