using AutoMapper;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Application.Services.Common
{
    public abstract class ServiceBase<TEntity, TID, TDto, TInsertDto, TUpdateDto> : IServiceBase<TEntity, TID, TDto, TInsertDto, TUpdateDto>
     where TEntity : class
     where TDto : class
     where TInsertDto : class
     where TUpdateDto : class
    {
        private readonly IGenericRepository<TEntity> _repository;
        private readonly IMapper _mapper;

        protected ServiceBase(IGenericRepository<TEntity> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async virtual Task<PaginatedResponse<TDto>> Paginate(Pagination pagination)
        {
            var paginatedEntities = await _repository.Paginate(pagination);
            var paginatedDtos = paginatedEntities.Map(_mapper.Map<TDto>);
            return paginatedDtos;
        }

        public async virtual Task<Result<TDto>> AddAsync(TInsertDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            var resPreProcessing = await InsertPreProcessing(entity, dto);
            if (!resPreProcessing.IsSuccess)
            {
                return Result.Failure<TDto>(resPreProcessing.Error.ToList());
            }
            await _repository.AddAsync(entity);
            var resultDto = _mapper.Map<TDto>(entity);
            return Result<TDto>.Success(resultDto);
        }

        protected async virtual Task<Result<TEntity>> InsertPreProcessing(TEntity entity, TInsertDto dto)
        {
            // Implement any necessary pre-processing logic here
            return Result<TEntity>.Success(entity);
        }

        public async virtual Task<Result> DeleteAsync(TID id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.Failure(new List<string> { "Entity not found." });
            }
            await _repository.RemoveAsync(entity);
            return Result.Success();

        }

        public async virtual Task<IEnumerable<TDto>> GetAllAsync()
            => (await _repository.GetAllAsync()).Select(_mapper.Map<TDto>);

        public async virtual Task<Result<TDto>> GetByIdAsync(TID id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.Failure<TDto>(new List<string> { "Entity not found." });
            }
            var dto = _mapper.Map<TDto>(entity);
            return Result<TDto>.Success(dto);
        }

        public async virtual Task<Result<TDto>> UpdateAsync(TID id, TUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.Failure<TDto>(new List<string> { "Entity not found." });
            }
            var mapped = _mapper.Map(dto, entity);
            var resPreProcessing = await UpdatePreProcessing(mapped, dto);
            if (!resPreProcessing.IsSuccess)
            {
                return Result.Failure<TDto>(resPreProcessing.Error.ToList());
            }
            await _repository.UpdateAsync(mapped);
            var resultDto = _mapper.Map<TDto>(mapped);
            return Result<TDto>.Success(resultDto);
        }

        protected async virtual Task<Result<TEntity>> UpdatePreProcessing(TEntity entity, TUpdateDto dto)
        {
            // Implement any necessary pre-processing logic here
            return Result<TEntity>.Success(entity);
        }
    }
}
