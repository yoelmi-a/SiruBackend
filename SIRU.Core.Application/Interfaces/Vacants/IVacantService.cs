using SIRU.Core.Application.Dtos.Vacants;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Interfaces.Vacants
{
    public interface IVacantService : IServiceBase<Vacant, string, VacantDto, SaveVacantDto, UpdateVacantDto>
    {
    }
}