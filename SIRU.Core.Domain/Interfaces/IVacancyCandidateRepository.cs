using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Domain.Interfaces;

public interface IVacancyCandidateRepository : IGenericRepository<VacancyCandidate>
{
    Task<VacancyCandidate?> GetByIdWithDetailsAsync(string id);
    Task<IEnumerable<VacancyCandidate>> GetAllByVacancyIdAsync(string vacancyId);
    Task<PaginatedResponse<VacancyCandidate>> GetPaginatedByVacancyIdAsync(string vacancyId, Pagination pagination);
}