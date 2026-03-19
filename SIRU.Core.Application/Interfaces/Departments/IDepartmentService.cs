using SIRU.Core.Application.Dtos.Department;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Interfaces.Departments
{
    public interface IDepartmentService : IServiceBase<Department, int, DepartmentDto, DepartmentInsertDto, DepartmentUpdateDto>
    {
    }
}
