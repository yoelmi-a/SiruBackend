using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Domain.Interfaces;

public interface IVacancyCandidateRepository : IGenericRepository<VacancyCandidate>
{
    Task<VacancyCandidate?> GetByIdWithDetailsAsync(string id);
}