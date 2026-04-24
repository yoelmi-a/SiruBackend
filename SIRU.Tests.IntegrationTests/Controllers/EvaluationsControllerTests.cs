using System.Text.Json.Serialization;
using SIRU.Core.Application.Dtos.Departments;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Dtos.Employees;
using SIRU.Core.Application.Dtos.Positions;
using System.Net;
using System.Net.Http.Json;

namespace SIRU.Tests.IntegrationTests.Controllers;

public class EvaluationsControllerTests : IntegrationTestBase
{
    private HttpClient _client = null!;

    public EvaluationsControllerTests()
    {
        _client = CreateDefaultClient();
    }

    private async Task<string> SeedEmployeeWithPositionAsync()
    {
        var deptResponse = await _client.PostAsJsonAsync("/api/departments", new DepartmentInsertDto { Name = "Sales" });
        var dept = await deptResponse.Content.ReadFromJsonAsync<DepartmentDto>();

        var posResponse = await _client.PostAsJsonAsync("/api/positions", new PositionInsertDto
        {
            Name = "Sales Rep",
            Salary = 50000m,
            DepartmentId = dept!.Id
        });
        var pos = await posResponse.Content.ReadFromJsonAsync<PositionDto>();

        var empDto = new EmployeeInsertDto
        {
            FirstName = "Eva",
            LastName = "Green",
            Address = "789 Pine St",
            Cedula = $"EVA{Guid.NewGuid().ToString()[..6].ToUpper()}",
            PhoneNumber = "5559998888",
            DateOfBirth = DateTime.UtcNow,
            Email = "eva.green@test.com"
        };
        var empResponse = await _client.PostAsJsonAsync("/api/employees", empDto);
        var emp = await empResponse.Content.ReadFromJsonAsync<EmployeeDto>();

        var historyDto = new EmployeePositionInsertDto()
        {
            EmployeeId = emp!.Id,
            PositionId = pos!.Id,
            StartDate = DateTime.UtcNow
        };
        await _client.PostAsJsonAsync($"/api/employees/{emp!.Id}/positions", historyDto);

        return emp.Id;
    }

    private async Task<int> SeedCriterionAsync(string name = "Teamwork")
    {
        var response = await _client.PostAsJsonAsync("/api/evaluation-criteria", new CriterionInsertDto { Name = name });
        var result = await response.Content.ReadFromJsonAsync<CriterionDto>();
        return result!.Id;
    }

    #region POST /api/evaluations

    [Fact]
    public async Task Create_WithValidData_Returns201Created()
    {
        var employeeId = await SeedEmployeeWithPositionAsync();
        var criterionId = await SeedCriterionAsync("Communication");

        var dto = new EvaluationInsertDto
        {
            EmployeeId = employeeId,
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>
            {
                new EvaluationCriterionInsertDto { CriterionId = criterionId, Score = 4.0f, Observation = "Good work" }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/evaluations", dto);

        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<EvaluationDto>();
        Assert.NotNull(result);
        Assert.Equal(4.0f, result.AverageScore);
    }

    [Fact]
    public async Task Create_WithEmptyCriteria_Returns400BadRequest()
    {
        var employeeId = await SeedEmployeeWithPositionAsync();

        var dto = new EvaluationInsertDto
        {
            EmployeeId = employeeId,
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>()
        };

        var response = await _client.PostAsJsonAsync("/api/evaluations", dto);

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Create_WithScoreOutOfRange_Returns400BadRequest()
    {
        var employeeId = await SeedEmployeeWithPositionAsync();
        var criterionId = await SeedCriterionAsync("Leadership");

        var dto = new EvaluationInsertDto
        {
            EmployeeId = employeeId,
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>
            {
                new EvaluationCriterionInsertDto { CriterionId = criterionId, Score = 6.0f, Observation = "Over maximum" }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/evaluations", dto);

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Create_WithNonExistentEmployeeId_Returns404NotFound()
    {
        var criterionId = await SeedCriterionAsync("Punctuality");

        var dto = new EvaluationInsertDto
        {
            EmployeeId = "nonexistent-id",
            EvaluationDate = new DateTime(2025, 3, 1),
            Criteria = new List<EvaluationCriterionInsertDto>
            {
                new EvaluationCriterionInsertDto { CriterionId = criterionId, Score = 3.5f, Observation = "" }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/evaluations", dto);

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    private class DepartmentDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }

    private class PositionDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("salary")]
        public decimal Salary { get; set; }
        [JsonPropertyName("departmentId")]
        public int DepartmentId { get; set; }
        [JsonPropertyName("departmentName")]
        public string? DepartmentName { get; set; }
    }

    private class EmployeeDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [JsonPropertyName("firstName")]
        public required string FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public required string LastName { get; set; }
        [JsonPropertyName("address")]
        public required string Address { get; set; }
        [JsonPropertyName("cedula")]
        public required string Cedula { get; set; }
        [JsonPropertyName("phoneNumber")]
        public required string PhoneNumber { get; set; }
        [JsonPropertyName("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }

    private class CriterionDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }

    private class EvaluationDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [JsonPropertyName("employeeId")]
        public required string EmployeeId { get; set; }
        [JsonPropertyName("employeeFullName")]
        public required string EmployeeFullName { get; set; }
        [JsonPropertyName("positionName")]
        public required string PositionName { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        [JsonPropertyName("averageScore")]
        public float AverageScore { get; set; }
    }
}