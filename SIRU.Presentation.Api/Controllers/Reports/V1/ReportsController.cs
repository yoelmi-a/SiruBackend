using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Application.Interfaces.Reports;
using SIRU.Core.Domain.Common.Pagination;

namespace SIRU.Presentation.Api.Controllers.Reports.V1;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("hiring-time")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HiringTimeReportDto))]
    public async Task<IActionResult> GetHiringTime()
    {
        var result = await _reportService.GetAverageHiringTimeAsync();
        return Ok(result);
    }

    [HttpGet("performance-by-department")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DepartmentPerformanceDto>))]
    public async Task<IActionResult> GetPerformanceByDepartment()
    {
        var result = await _reportService.GetPerformanceByDepartmentAsync();
        return Ok(result);
    }

    [HttpGet("employees")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<EmployeeReportDto>))]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null)
    {
        var pagination = new Pagination { PageNumber = page, PageSize = pageSize };
        var result = await _reportService.GetEmployeeReportAsync(pagination, isActive);
        return Ok(result);
    }

    [HttpGet("hiring-time/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportHiringTime()
    {
        try
        {
            var pdf = await _reportService.ExportHiringTimeAsync();
            return File(pdf, "application/pdf", "hiring-time-report.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate hiring time PDF");
            return StatusCode(500, new { message = "PDF generation failed. Please try again later." });
        }
    }

    [HttpGet("performance-by-department/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportPerformanceByDepartment()
    {
        try
        {
            var pdf = await _reportService.ExportPerformanceByDepartmentAsync();
            return File(pdf, "application/pdf", "performance-by-department-report.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate performance by department PDF");
            return StatusCode(500, new { message = "PDF generation failed. Please try again later." });
        }
    }

    [HttpGet("employees/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportEmployees()
    {
        try
        {
            var pdf = await _reportService.ExportEmployeesAsync();
            return File(pdf, "application/pdf", "employees-report.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate employees PDF");
            return StatusCode(500, new { message = "PDF generation failed. Please try again later." });
        }
    }
}