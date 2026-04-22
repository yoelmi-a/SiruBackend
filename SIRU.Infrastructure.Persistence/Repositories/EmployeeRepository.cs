using Microsoft.EntityFrameworkCore;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infrastructure.Persistence.Repositories;

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

    public async Task<EmployeePosition?> GetCurrentPositionAsync(string employeeId)
    {
        return await _dbSet
            .Where(e => e.Id == employeeId)
            .SelectMany(e => e.PositionsOccupied!)
            .Where(ep => ep.EndDate == null)
            .Include(ep => ep.Position!)
                .ThenInclude(p => p.Department!)
            .OrderByDescending(ep => ep.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Evaluation>> GetEmployeeEvaluationsAsync(string employeeId)
    {
        return await _dbSet
            .Where(e => e.Id == employeeId)
            .SelectMany(e => e.PositionsOccupied!)
            .SelectMany(ep => ep.Evaluations!)
            .Include(ev => ev.Criteria!)
                .ThenInclude(ec => ec.Criterion!)
            .OrderByDescending(ev => ev.Date)
            .ToListAsync();
    }
}