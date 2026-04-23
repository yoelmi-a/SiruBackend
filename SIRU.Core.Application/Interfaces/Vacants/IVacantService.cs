using SIRU.Core.Application.Dtos.Vacants;
using SIRU.Core.Application.Dtos.Vacancies;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Interfaces.Vacants;

public interface IVacantService : IServiceBase<Vacant, string, VacantDto, SaveVacantDto, UpdateVacantDto>
{
    Task<Result<VacancyApplicationResultDto>> ApplyToVacancyAsync(string vacantId, VacancyApplicationDto dto);
    Task<Result> RecalculateScoresAsync(string vacantId);
    Task<Result<PaginatedResponse<VacancyApplicationResultDto>>> GetApplicationsByVacancyAsync(string vacantId, Pagination pagination);
}