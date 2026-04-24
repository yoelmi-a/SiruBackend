using System.Text.Json.Serialization;
using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Domain.Common.Pagination;
using System.Net;
using System.Net.Http.Json;

namespace SIRU.Tests.IntegrationTests.Controllers;

public class ReportsControllerTests : IntegrationTestBase
{
    private HttpClient _client = null!;

    public ReportsControllerTests()
    {
        _client = CreateDefaultClient();
    }

    #region GET /api/reports/hiring-time

    [Fact]
    public async Task GetHiringTime_WithNoData_ReturnsZeros()
    {
        var response = await _client.GetAsync("/api/reports/hiring-time");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<HiringTimeReportDto>();
        Assert.NotNull(result);
        Assert.Equal(0, result.AverageDays);
        Assert.Equal(0, result.TotalClosedVacancies);
    }

    #endregion

    #region GET /api/reports/performance-by-department

    [Fact]
    public async Task GetPerformanceByDepartment_WithNoData_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/reports/performance-by-department");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GET /api/reports/employees

    [Fact]
    public async Task GetEmployees_WithPagination_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/reports/employees?page=1&pageSize=10");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<EmployeeReportDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    #endregion

    #region GET /api/reports/hiring-time/export

    [Fact]
    public async Task ExportHiringTime_ReturnsPdfFile()
    {
        var response = await _client.GetAsync("/api/reports/hiring-time/export");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("hiring-time-report.pdf", response.Content.Headers.ContentDisposition?.FileName);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 0, "PDF content should not be empty");
        Assert.Equal(0x25, bytes[0]);
    }

    #endregion

    #region GET /api/reports/performance-by-department/export

    [Fact]
    public async Task ExportPerformanceByDepartment_ReturnsPdfFile()
    {
        var response = await _client.GetAsync("/api/reports/performance-by-department/export");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("performance-by-department-report.pdf", response.Content.Headers.ContentDisposition?.FileName);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 0, "PDF content should not be empty");
        Assert.Equal(0x25, bytes[0]);
    }

    #endregion

    #region GET /api/reports/employees/export

    [Fact]
    public async Task ExportEmployees_ReturnsPdfFile()
    {
        var response = await _client.GetAsync("/api/reports/employees/export");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("employees-report.pdf", response.Content.Headers.ContentDisposition?.FileName);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 0, "PDF content should not be empty");
        Assert.Equal(0x25, bytes[0]);
    }

    #endregion

    private class HiringTimeReportDto
    {
        [JsonPropertyName("averageDays")]
        public float AverageDays { get; set; }
        [JsonPropertyName("totalClosedVacancies")]
        public int TotalClosedVacancies { get; set; }
    }

    private class EmployeeReportDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [JsonPropertyName("fullName")]
        public required string FullName { get; set; }
        [JsonPropertyName("cedula")]
        public required string Cedula { get; set; }
        [JsonPropertyName("position")]
        public required string Position { get; set; }
        [JsonPropertyName("department")]
        public required string Department { get; set; }
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }

    private class PaginatedResponse<T>
    {
        [JsonPropertyName("items")]
        public required IEnumerable<T> Items { get; set; }
        [JsonPropertyName("pagination")]
        public required Pagination Pagination { get; set; }
    }

    private class Pagination
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }
}