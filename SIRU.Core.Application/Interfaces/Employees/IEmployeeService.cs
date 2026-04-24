using SIRU.Core.Application.Dtos.Employees;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Interfaces.Employees
{
    public interface IEmployeeService : IServiceBase<Employee, string, EmployeeDto, EmployeeInsertDto, EmployeeUpdateDto>
    {
        Task<PaginatedResponse<EmployeeListDto>> Paginate(Pagination pagination, bool? isActive = null);
        Task<Result<IEnumerable<EmployeeListDto>>> GetAllAsync(bool? isActive = null);
        Task<Result<IEnumerable<EmployeeHistoryDto>>> GetHistoryAsync(string employeeId);
        Task<Result<EmployeePositionDto>> AssignPositionAsync(EmployeePositionInsertDto dto);
    }
}