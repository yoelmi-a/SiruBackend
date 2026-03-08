using AutoMapper;
using SIRU.Core.Application.Dtos.Vacant;
using SIRU.Core.Application.Interfaces.Vacant;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Vacant
{
    public class VacantService : IVacantService
    {
        private readonly IGenericRepository<SIRU.Core.Domain.Entities.Vacant> _repository;
        private readonly IMapper _mapper;

        public VacantService(IGenericRepository<SIRU.Core.Domain.Entities.Vacant> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<VacantDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<VacantDto>>(entities);
            return Result.Success(dtos);
        }

        public async Task<Result<VacantDto>> GetByIdAsync(string id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.Failure<VacantDto>(["Vacant not found"]);
            }
            var dto = _mapper.Map<VacantDto>(entity);
            return Result.Success(dto);
        }

        public async Task<Result<VacantDto>> AddAsync(SaveVacantDto dto)
        {
            var entity = _mapper.Map<SIRU.Core.Domain.Entities.Vacant>(dto);
            entity.Id = Guid.NewGuid().ToString();
            entity.PublicationDate = DateTime.UtcNow;
            
            await _repository.AddAsync(entity);
            var resultDto = _mapper.Map<VacantDto>(entity);
            return Result.Success(resultDto);
        }

        public async Task<Result> UpdateAsync(string id, SaveVacantDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.Failure(["Vacant not found"]);
            }

            _mapper.Map(dto, entity);
            entity.Id = id;

            _repository.Update(entity);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(string id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.Failure(["Vacant not found"]);
            }

            _repository.Remove(entity);
            return Result.Success();
        }
    }
}
