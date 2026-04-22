using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infrastructure.Persistence.Repositories;

public class EvaluationRepository : GenericRepository<Evaluation>, IEvaluationRepository
{
    public EvaluationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}