using AutoMapper;
using SIRU.Core.Application.Dtos.Department;
using SIRU.Core.Application.Interfaces.Departments;
using SIRU.Core.Application.Services.Common;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Departments
{
    public class DepartmentService : ServiceBase<Department, int, DepartmentDto, DepartmentInsertDto, DepartmentUpdateDto>, IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository, IMapper mapper) : base(departmentRepository, mapper)
        {
            _departmentRepository = departmentRepository;
        }

        protected override async Task<Result<Department>> InsertPreProcessing(Department entity, DepartmentInsertDto dto)
        {
            var exists = await _departmentRepository.FindAsync(d => d.Name.ToLower() == dto.Name.ToLower());
            if (exists.Any())
            {
                return Result.Failure<Department>(new List<string> { $"Ya existe un departamento con el nombre '{dto.Name}'." });
            }

            return await base.InsertPreProcessing(entity, dto);
        }

        protected override async Task<Result<Department>> UpdatePreProcessing(Department entity, DepartmentUpdateDto dto)
        {
            var exists = await _departmentRepository.FindAsync(d => d.Name.ToLower() == dto.Name.ToLower() && d.Id != entity.Id);
            if (exists.Any())
            {
                return Result.Failure<Department>(new List<string> { $"Ya existe otro departamento con el nombre '{dto.Name}'." });
            }

            return await base.UpdatePreProcessing(entity, dto);
        }
    }
}
