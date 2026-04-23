using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Domain.Common.Pagination;

namespace SIRU.Core.Application.Interfaces.Reports;

public interface IReportRepository
{
    Task<HiringTimeReportDto> GetAverageHiringTimeAsync();
    Task<IEnumerable<DepartmentPerformanceDto>> GetPerformanceByDepartmentAsync();
    Task<PaginatedResponse<EmployeeReportDto>> GetEmployeeReportAsync(Pagination pagination, bool? isActive);
    Task<IEnumerable<EmployeeReportDto>> GetAllEmployeesAsync();
}