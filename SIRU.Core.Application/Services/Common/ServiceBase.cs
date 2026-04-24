using Mapster;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Common.Enums;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Common
{
    public abstract class ServiceBase<TEntity, TID, TDto, TInsertDto, TUpdateDto> : IServiceBase<TEntity, TID, TDto, TInsertDto, TUpdateDto>
     where TEntity : class
     where TDto : class
     where TInsertDto : class
     where TUpdateDto : class
    {
        protected readonly IGenericRepository<TEntity> _repository;

        protected ServiceBase(IGenericRepository<TEntity> repository)
        {
            _repository = repository;
        }

        public async virtual Task<PaginatedResponse<TDto>> Paginate(Pagination pagination)
        {
            var paginatedEntities = await _repository.Paginate(pagination);
            var paginatedDtos = new PaginatedResponse<TDto>
            {
                Items = paginatedEntities.Items.Select(e => e.Adapt<TDto>()),
                Pagination = paginatedEntities.Pagination
            };
            return paginatedDtos;
        }

        public async virtual Task<Result<TDto>> AddAsync(TInsertDto dto)
        {
            var entity = dto.Adapt<TEntity>();
            var resPreProcessing = await InsertPreProcessing(entity, dto);
            if (!resPreProcessing.IsSuccess)
            {
                return Result.Failure<TDto>(resPreProcessing.Errors.ToList(), resPreProcessing.ErrorTypeCode ?? ErrorType.BadRequest);
            }
            await _repository.AddAsync(entity);
            var resultDto = entity.Adapt<TDto>();
            return Result<TDto>.Success(resultDto);
        }

        protected async virtual Task<Result<TEntity>> InsertPreProcessing(TEntity entity, TInsertDto dto)
        {
            return Result<TEntity>.Success(entity);
        }

        public async virtual Task<Result> DeleteAsync(TID id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.NotFound("Entity not found.");
            }
            await _repository.RemoveAsync(entity);
            return Result.Success();
        }

        public async virtual Task<IEnumerable<TDto>> GetAllAsync()
            => (await _repository.GetAllAsync()).Select(e => e.Adapt<TDto>());

        public async virtual Task<Result<TDto>> GetByIdAsync(TID id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.NotFound<TDto>(new List<string> { "Entity not found." });
            }
            var dto = entity.Adapt<TDto>();
            return Result<TDto>.Success(dto);
        }

        public async virtual Task<Result<TDto>> UpdateAsync(TID id, TUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.NotFound<TDto>(new List<string> { "Entity not found." });
            }
            dto.Adapt(entity);
            var resPreProcessing = await UpdatePreProcessing(entity, dto);
            if (!resPreProcessing.IsSuccess)
            {
                return Result.Failure<TDto>(resPreProcessing.Errors.ToList(), resPreProcessing.ErrorTypeCode ?? ErrorType.BadRequest);
            }
            await _repository.UpdateAsync(entity);
            var resultDto = entity.Adapt<TDto>();
            return Result<TDto>.Success(resultDto);
        }

        protected async virtual Task<Result<TEntity>> UpdatePreProcessing(TEntity entity, TUpdateDto dto)
        {
            return Result<TEntity>.Success(entity);
        }
    }
}