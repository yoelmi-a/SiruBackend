using AutoMapper;
using SIRU.Core.Application.Interfaces.Generic;
using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Application.Services.Generic
{
    public class GenericService<TEntity, TDto, TKey> : IGenericService<TEntity, TDto, TKey>
        where TEntity : BaseEntity<TKey>
        where TDto : class
    {
        private readonly IGenericRepository<TEntity> _repository;
        private readonly IMapper _mapper;

        public GenericService(IGenericRepository<TEntity> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<TDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TDto>>(entities);
            return Result.Success(dtos);
        }

        public async Task<Result<TDto>> AddAsync(TDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);

            await _repository.AddAsync(entity);
            var resultDto = _mapper.Map<TDto>(entity);
            return Result.Success(resultDto);
        }

        public async Task<Result<TDto>> GetByIdAsync(TKey id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.Failure<TDto>(new List<string> { "Entity not found" });
            }
            var dto = _mapper.Map<TDto>(entity);
            return Result.Success(dto);
        }

        public async Task<Result> UpdateAsync(TKey id, TDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.Failure(new List<string> { "Entity not found" });
            }

            _mapper.Map(dto, entity);
            entity.Id = id;

            _repository.Update(entity);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(TKey id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return Result.Failure(new List<string> { "Entity not found" });
            }

            _repository.Remove(entity);
            return Result.Success();
        }
    }
}
