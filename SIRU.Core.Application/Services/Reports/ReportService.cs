using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Application.Interfaces.Reports;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Reports;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<HiringTimeReportDto> GetAverageHiringTimeAsync()
        => await _reportRepository.GetAverageHiringTimeAsync();

    public async Task<IEnumerable<DepartmentPerformanceDto>> GetPerformanceByDepartmentAsync()
        => await _reportRepository.GetPerformanceByDepartmentAsync();

    public async Task<PaginatedResponse<EmployeeReportDto>> GetEmployeeReportAsync(Pagination pagination, bool? isActive)
        => await _reportRepository.GetEmployeeReportAsync(pagination, isActive);
}