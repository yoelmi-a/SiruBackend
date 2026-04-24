using System.Text.Json.Serialization;
using SIRU.Core.Application.Dtos.Departments;
using SIRU.Core.Application.Dtos.Positions;
using System.Net;
using System.Net.Http.Json;

namespace SIRU.Tests.IntegrationTests.Controllers;

public class PositionsControllerTests : IntegrationTestBase
{
    private HttpClient _client = null!;

    public PositionsControllerTests()
    {
        _client = CreateDefaultClient();
    }

    private async Task<int> SeedDepartmentAsync(string name = "IT Department")
    {
        var response = await _client.PostAsJsonAsync("/api/departments", new DepartmentInsertDto { Name = name });
        var result = await response.Content.ReadFromJsonAsync<DepartmentDto>();
        return result!.Id;
    }

    #region GET /api/positions

    [Fact]
    public async Task GetAll_WithNoPositions_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/positions");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var positions = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(positions);
        Assert.Empty(positions);
    }

    #endregion

    #region POST /api/positions

    [Fact]
    public async Task Create_WithValidData_Returns201Created()
    {
        var departmentId = await SeedDepartmentAsync("Engineering");

        var dto = new PositionInsertDto
        {
            Name = "Senior Developer",
            Salary = 50000m,
            DepartmentId = departmentId
        };
        var response = await _client.PostAsJsonAsync("/api/positions", dto);

        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PositionDto>();
        Assert.NotNull(result);
        Assert.Equal("Senior Developer", result.Name);
        Assert.Equal(50000m, result.Salary);
        Assert.Equal(departmentId, result.DepartmentId);
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns400BadRequest()
    {
        var departmentId = await SeedDepartmentAsync("HR");

        var dto = new PositionInsertDto
        {
            Name = "",
            Salary = 30000m,
            DepartmentId = departmentId
        };
        var response = await _client.PostAsJsonAsync("/api/positions", dto);

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Create_WithZeroSalary_Returns400BadRequest()
    {
        var departmentId = await SeedDepartmentAsync("Admin");

        var dto = new PositionInsertDto
        {
            Name = "Manager",
            Salary = 0m,
            DepartmentId = departmentId
        };
        var response = await _client.PostAsJsonAsync("/api/positions", dto);

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    #endregion

    #region GET /api/positions/{id}

    [Fact]
    public async Task GetById_WithExistingId_Returns200Ok()
    {
        var departmentId = await SeedDepartmentAsync("Finance");
        var createResponse = await _client.PostAsJsonAsync("/api/positions", new PositionInsertDto
        {
            Name = "Accountant",
            Salary = 45000m,
            DepartmentId = departmentId
        });
        var created = await createResponse.Content.ReadFromJsonAsync<PositionDto>();

        var response = await _client.GetAsync($"/api/positions/{created!.Id}");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PositionDto>();
        Assert.NotNull(result);
        Assert.Equal("Accountant", result.Name);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/positions/99999");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region PUT /api/positions/{id}

    [Fact]
    public async Task Update_WithExistingId_Returns204NoContent()
    {
        var departmentId = await SeedDepartmentAsync("Legal");
        var createResponse = await _client.PostAsJsonAsync("/api/positions", new PositionInsertDto
        {
            Name = "Paralegal",
            Salary = 35000m,
            DepartmentId = departmentId
        });
        var created = await createResponse.Content.ReadFromJsonAsync<PositionDto>();

        var updateDto = new PositionUpdateDto
        {
            Name = "Senior Paralegal",
            Salary = 40000m,
            DepartmentId = departmentId
        };
        var response = await _client.PutAsJsonAsync($"/api/positions/{created!.Id}", updateDto);

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingId_Returns404NotFound()
    {
        var departmentId = await SeedDepartmentAsync("Operations");
        var updateDto = new PositionUpdateDto
        {
            Name = "Director",
            Salary = 80000m,
            DepartmentId = departmentId
        };

        var response = await _client.PutAsJsonAsync($"/api/positions/99999", updateDto);

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region DELETE /api/positions/{id}

    [Fact]
    public async Task Delete_WithExistingId_Returns204NoContent()
    {
        var departmentId = await SeedDepartmentAsync("Temp Dept");
        var createResponse = await _client.PostAsJsonAsync("/api/positions", new PositionInsertDto
        {
            Name = "Temp Position",
            Salary = 25000m,
            DepartmentId = departmentId
        });
        var created = await createResponse.Content.ReadFromJsonAsync<PositionDto>();

        var response = await _client.DeleteAsync($"/api/positions/{created!.Id}");

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.DeleteAsync("/api/positions/99999");

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
}