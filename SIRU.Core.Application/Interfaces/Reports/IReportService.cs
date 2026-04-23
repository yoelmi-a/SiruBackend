using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Domain.Common.Pagination;

namespace SIRU.Core.Application.Interfaces.Reports;

public interface IReportService
{
    Task<HiringTimeReportDto> GetAverageHiringTimeAsync();
    Task<IEnumerable<DepartmentPerformanceDto>> GetPerformanceByDepartmentAsync();
    Task<PaginatedResponse<EmployeeReportDto>> GetEmployeeReportAsync(Pagination pagination, bool? isActive);
    Task<byte[]> ExportHiringTimeAsync();
    Task<byte[]> ExportPerformanceByDepartmentAsync();
    Task<byte[]> ExportEmployeesAsync();
}