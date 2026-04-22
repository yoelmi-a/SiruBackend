using Mapster;
using SIRU.Core.Application.Dtos.Vacants;
using SIRU.Core.Application.Interfaces.Vacants;
using SIRU.Core.Application.Services.Common;
using SIRU.Core.Domain.Common.Enums;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Vacants
{
    public class VacantService : ServiceBase<Vacant, string, VacantDto, SaveVacantDto, UpdateVacantDto>, IVacantService
    {
        public VacantService(IGenericRepository<Vacant> repository) : base(repository)
        {
        }

        protected override async Task<Result<Vacant>> InsertPreProcessing(Vacant entity, SaveVacantDto dto)
        {
            entity.Id = Guid.NewGuid().ToString();
            entity.PublicationDate = DateTime.UtcNow;
            entity.Status = VacantStatus.Open;
            return Result<Vacant>.Success(entity);
        }

        protected override async Task<Result<Vacant>> UpdatePreProcessing(Vacant entity, UpdateVacantDto dto)
        {
            return Result<Vacant>.Success(entity);
        }
    }
}