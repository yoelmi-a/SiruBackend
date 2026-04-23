using SIRU.Core.Application.Dtos.Reports;

namespace SIRU.Core.Application.Interfaces.Reports;

public interface IPdfReportService
{
    byte[] GenerateHiringTimeReport(HiringTimeReportDto data);
    byte[] GeneratePerformanceByDepartmentReport(IEnumerable<DepartmentPerformanceDto> data);
    byte[] GenerateEmployeeReport(IEnumerable<EmployeeReportDto> data);
}