using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infrastructure.Persistence.Repositories;

public class VacantRepository : GenericRepository<Vacant>, IVacantRepository
{
    public VacantRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}