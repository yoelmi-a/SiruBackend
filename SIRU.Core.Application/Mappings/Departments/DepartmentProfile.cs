using AutoMapper;
using SIRU.Core.Application.Dtos.Department;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Mappings.Departments
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<DepartmentInsertDto, Department>();
            CreateMap<DepartmentUpdateDto, Department>();
        }
    }
}
