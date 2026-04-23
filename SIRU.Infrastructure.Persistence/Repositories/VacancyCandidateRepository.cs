using Microsoft.EntityFrameworkCore;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infrastructure.Persistence.Repositories;

public class VacancyCandidateRepository : GenericRepository<VacancyCandidate>, IVacancyCandidateRepository
{
    public VacancyCandidateRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<VacancyCandidate?> GetByIdWithDetailsAsync(string id)
    {
        return await _dbSet
            .Include(vc => vc.Vacant)
            .Include(vc => vc.Candidate)
            .FirstOrDefaultAsync(vc => vc.Id == id);
    }

    public async Task<IEnumerable<VacancyCandidate>> GetAllByVacancyIdAsync(string vacancyId)
    {
        return await _dbSet
            .Include(vc => vc.Candidate)
            .Where(vc => vc.VacantId == vacancyId)
            .OrderByDescending(vc => vc.Score)
            .ToListAsync();
    }

    public async Task<PaginatedResponse<VacancyCandidate>> GetPaginatedByVacancyIdAsync(string vacancyId, Pagination pagination)
    {
        var query = _dbSet
            .Include(vc => vc.Candidate)
            .Where(vc => vc.VacantId == vacancyId)
            .OrderByDescending(vc => vc.Score);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginatedResponse<VacancyCandidate>
        {
            Items = items,
            Pagination = new Pagination
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            }
        };
    }
}