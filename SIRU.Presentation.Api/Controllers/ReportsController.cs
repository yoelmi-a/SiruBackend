using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Application.Interfaces.Reports;
using SIRU.Core.Domain.Common.Pagination;

namespace SIRU.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
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
}