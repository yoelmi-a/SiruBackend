using AutoMapper;
using SIRU.Core.Application.Dtos.Position;
using SIRU.Core.Application.Interfaces.Positions;
using SIRU.Core.Application.Services.Common;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Positions
{
    public class PositionService : ServiceBase<Position, int, PositionDto, PositionInsertDto, PositionUpdateDto>, IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public PositionService(IPositionRepository positionRepository, IDepartmentRepository departmentRepository, IMapper mapper) : base(positionRepository, mapper)
        {
            _positionRepository = positionRepository;
            _departmentRepository = departmentRepository;
        }

        protected override async Task<Result<Position>> InsertPreProcessing(Position entity, PositionInsertDto dto)
        {
            var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
            if (department == null)
            {
                return Result.Failure<Position>(new List<string> { $"No se encontró el departamento con ID {dto.DepartmentId}." });
            }

            return await base.InsertPreProcessing(entity, dto);
        }

        protected override async Task<Result<Position>> UpdatePreProcessing(Position entity, PositionUpdateDto dto)
        {
            var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
            if (department == null)
            {
                return Result.Failure<Position>(new List<string> { $"No se encontró el departamento con ID {dto.DepartmentId}." });
            }

            return await base.UpdatePreProcessing(entity, dto);
        }
    }
}
