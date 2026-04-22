using Microsoft.EntityFrameworkCore;
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
}