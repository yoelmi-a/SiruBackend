using System.Text.Json.Serialization;
using SIRU.Core.Application.Dtos.Employees;
using SIRU.Core.Domain.Common.Pagination;
using System.Net;
using System.Net.Http.Json;

namespace SIRU.Tests.IntegrationTests.Controllers;

public class EmployeesControllerTests : IntegrationTestBase
{
    private HttpClient _client = null!;

    public EmployeesControllerTests()
    {
        _client = CreateDefaultClient();
    }

    private async Task<string> SeedEmployeeAsync(string firstName = "John", string lastName = "Doe", string cedulaSuffix = "")
    {
        var dto = new EmployeeInsertDto
        {
            FirstName = firstName,
            LastName = lastName,
            Address = "123 Main St",
            Cedula = $"EMP{Guid.NewGuid().ToString()[..6].ToUpper()}{cedulaSuffix}",
            PhoneNumber = "1234567890",
            DateOfBirth = DateTime.UtcNow,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@test.com"
        };

        var response = await _client.PostAsJsonAsync("/api/employees", dto);
        var result = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        return result!.Id;
    }

    #region GET /api/employees

    [Fact]
    public async Task GetAll_WithNoEmployees_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/employees");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<EmployeeListDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsPaginatedList()
    {
        await SeedEmployeeAsync("Alice", "A");
        await SeedEmployeeAsync("Bob", "B");

        var response = await _client.GetAsync("/api/employees?page=1&pageSize=10");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<EmployeeListDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Pagination.TotalCount);
    }

    #endregion

    #region GET /api/employees/{id}

    [Fact]
    public async Task GetById_WithExistingId_Returns200Ok()
    {
        var employeeId = await SeedEmployeeAsync("Charlie", "Smith");

        var response = await _client.GetAsync($"/api/employees/{employeeId}");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(result);
        Assert.Equal("Charlie", result.FirstName);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/employees/nonexistent-id");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region POST /api/employees

    [Fact]
    public async Task Create_WithValidData_Returns201Created()
    {
        var dto = new EmployeeInsertDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            Address = "456 Oak Ave",
            Cedula = $"JDD{Guid.NewGuid().ToString()[..6].ToUpper()}",
            PhoneNumber = "5551234567",
            DateOfBirth = DateTime.UtcNow,
            Email = "jane.doe@test.com"
        };

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        Assert.NotNull(result);
        Assert.Equal("Jane", result.FirstName);
    }

    [Fact]
    public async Task Create_WithMissingFields_Returns400BadRequest()
    {
        var dto = new EmployeeInsertDto { FirstName = "OnlyFirstName", LastName = null!, Address = null!, Cedula = null!, PhoneNumber = null!, DateOfBirth = DateTime.UnixEpoch, Email = null! };
        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Create_WithDuplicateCedula_Returns409Conflict()
    {
        var cedula = $"DUP{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var dto1 = new EmployeeInsertDto
        {
            FirstName = "First",
            LastName = "Employee",
            Address = "Address",
            Cedula = cedula,
            PhoneNumber = "111",
            DateOfBirth = DateTime.UtcNow,
            Email = "first.employee@test.com"
        };
        await _client.PostAsJsonAsync("/api/employees", dto1);

        var dto2 = new EmployeeInsertDto
        {
            FirstName = "Second",
            LastName = "Employee",
            Address = "Address 2",
            Cedula = cedula,
            PhoneNumber = "222",
            DateOfBirth = DateTime.UtcNow,
            Email = "second.employee@test.com"
        };
        var response = await _client.PostAsJsonAsync("/api/employees", dto2);

        Assert.Equal(StatusCodes.Status409Conflict, (int)response.StatusCode);
    }

    #endregion

    #region PUT /api/employees/{id}

    [Fact]
    public async Task Update_WithExistingId_Returns200Ok()
    {
        var employeeId = await SeedEmployeeAsync("Original", "Name");

        var updateDto = new EmployeeUpdateDto
        {
            FirstName = "Updated",
            LastName = "Name",
            Address = "New Address",
            Cedula = $"UP{Guid.NewGuid().ToString()[..6].ToUpper()}",
            PhoneNumber = "999",
            DateOfBirth = DateTime.UtcNow,
            Email = "updated@email.com"
        };
        var response = await _client.PutAsJsonAsync($"/api/employees/{employeeId}", updateDto);

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingId_Returns404NotFound()
    {
        var updateDto = new EmployeeUpdateDto
        {
            FirstName = "Any",
            LastName = "Name",
            Address = "Addr",
            Cedula = $"ANY{Guid.NewGuid().ToString()[..4].ToUpper()}",
            PhoneNumber = "123",
            DateOfBirth = DateTime.UtcNow,
            Email = "updated@email.com"
        };
        var response = await _client.PutAsJsonAsync("/api/employees/nonexistent-id", updateDto);

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region DELETE /api/employees/{id}

    [Fact]
    public async Task Delete_WithExistingId_Returns204NoContent()
    {
        var employeeId = await SeedEmployeeAsync("ToDelete", "User");

        var response = await _client.DeleteAsync($"/api/employees/{employeeId}");

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    #endregion

    #region GET /api/employees/{id}/history

    [Fact]
    public async Task GetHistory_WithExistingEmployee_Returns200Ok()
    {
        var employeeId = await SeedEmployeeAsync("History", "User");

        var response = await _client.GetAsync($"/api/employees/{employeeId}/history");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var history = await response.Content.ReadFromJsonAsync<List<EmployeeHistoryDto>>();
        Assert.NotNull(history);
    }

    [Fact]
    public async Task GetHistory_WithNonExistingEmployee_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/employees/nonexistent-id/history");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region GET /api/employees/{id}/evaluations

    [Fact]
    public async Task GetEvaluations_WithExistingEmployee_Returns200Ok()
    {
        var employeeId = await SeedEmployeeAsync("Eval", "User");

        var response = await _client.GetAsync($"/api/employees/{employeeId}/evaluations");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var evaluations = await response.Content.ReadFromJsonAsync<List<EvaluationHistoryDto>>();
        Assert.NotNull(evaluations);
    }

    [Fact]
    public async Task GetEvaluations_WithNonExistingEmployee_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/employees/nonexistent-id/evaluations");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

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

    private class EmployeeListDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [JsonPropertyName("fullName")]
        public required string FullName { get; set; }
        [JsonPropertyName("cedula")]
        public required string Cedula { get; set; }
        [JsonPropertyName("phoneNumber")]
        public required string PhoneNumber { get; set; }
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }

    private class EmployeeHistoryDto
    {
        [JsonPropertyName("positionName")]
        public required string PositionName { get; set; }
        [JsonPropertyName("departmentName")]
        public required string DepartmentName { get; set; }
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
    }

    private class EvaluationHistoryDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        [JsonPropertyName("averageScore")]
        public float AverageScore { get; set; }
        [JsonPropertyName("positionName")]
        public required string PositionName { get; set; }
    }
}