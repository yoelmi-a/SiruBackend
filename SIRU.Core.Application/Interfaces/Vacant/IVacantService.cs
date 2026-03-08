using SIRU.Core.Application.Dtos.Vacant;
using SIRU.Core.Domain.Common.Results;

namespace SIRU.Core.Application.Interfaces.Vacant
{
    public interface IVacantService
    {
        Task<Result<IEnumerable<VacantDto>>> GetAllAsync();
        Task<Result<VacantDto>> GetByIdAsync(string id);
        Task<Result<VacantDto>> AddAsync(SaveVacantDto dto);
        Task<Result> UpdateAsync(string id, SaveVacantDto dto);
        Task<Result> DeleteAsync(string id);
    }
}
