using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Application.Interfaces.Reports;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Reports;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IPdfReportService _pdfReportService;

    public ReportService(IReportRepository reportRepository, IPdfReportService pdfReportService)
    {
        _reportRepository = reportRepository;
        _pdfReportService = pdfReportService;
    }

    public async Task<HiringTimeReportDto> GetAverageHiringTimeAsync()
        => await _reportRepository.GetAverageHiringTimeAsync();

    public async Task<IEnumerable<DepartmentPerformanceDto>> GetPerformanceByDepartmentAsync()
        => await _reportRepository.GetPerformanceByDepartmentAsync();

    public async Task<PaginatedResponse<EmployeeReportDto>> GetEmployeeReportAsync(Pagination pagination, bool? isActive)
        => await _reportRepository.GetEmployeeReportAsync(pagination, isActive);

    public async Task<byte[]> ExportHiringTimeAsync()
    {
        var data = await _reportRepository.GetAverageHiringTimeAsync();
        return _pdfReportService.GenerateHiringTimeReport(data);
    }

    public async Task<byte[]> ExportPerformanceByDepartmentAsync()
    {
        var data = await _reportRepository.GetPerformanceByDepartmentAsync();
        return _pdfReportService.GeneratePerformanceByDepartmentReport(data);
    }

    public async Task<byte[]> ExportEmployeesAsync()
    {
        var data = await _reportRepository.GetAllEmployeesAsync();
        return _pdfReportService.GenerateEmployeeReport(data);
    }
}