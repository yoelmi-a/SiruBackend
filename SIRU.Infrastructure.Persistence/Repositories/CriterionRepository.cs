using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infrastructure.Persistence.Repositories;

public class CriterionRepository : GenericRepository<Criterion>, ICriterionRepository
{
    public CriterionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}