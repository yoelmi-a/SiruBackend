using System.Text.Json.Serialization;
using SIRU.Core.Application.Dtos.Departments;
using System.Net;
using System.Net.Http.Json;

namespace SIRU.Tests.IntegrationTests.Controllers;

public class DepartmentsControllerTests : IntegrationTestBase
{
    private HttpClient _client = null!;

    public DepartmentsControllerTests()
    {
        _client = CreateDefaultClient();
    }

    #region GET /api/departments

    [Fact]
    public async Task GetAll_WithNoDepartments_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/departments");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var departments = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(departments);
        Assert.Empty(departments);
    }

    #endregion

    #region POST /api/departments

    [Fact]
    public async Task Create_WithValidData_Returns201Created()
    {
        var dto = new DepartmentInsertDto { Name = "Engineering" };
        var response = await _client.PostAsJsonAsync("/api/departments", dto);

        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.NotNull(result);
        Assert.Equal("Engineering", result.Name);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns400BadRequest()
    {
        var dto = new DepartmentInsertDto { Name = "" };
        var response = await _client.PostAsJsonAsync("/api/departments", dto);

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Create_WithDuplicateName_Returns409Conflict()
    {
        var dto = new DepartmentInsertDto { Name = "HR Department" };
        await _client.PostAsJsonAsync("/api/departments", dto);

        var duplicateDto = new DepartmentInsertDto { Name = "HR Department" };
        var duplicateResponse = await _client.PostAsJsonAsync("/api/departments", duplicateDto);

        Assert.Equal(StatusCodes.Status409Conflict, (int)duplicateResponse.StatusCode);
    }

    #endregion

    #region GET /api/departments/{id}

    [Fact]
    public async Task GetById_WithExistingId_Returns200Ok()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/departments", new DepartmentInsertDto { Name = "Finance" });
        var created = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();

        var response = await _client.GetAsync($"/api/departments/{created!.Id}");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DepartmentDto>();
        Assert.NotNull(result);
        Assert.Equal("Finance", result.Name);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/departments/99999");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region PUT /api/departments/{id}

    [Fact]
    public async Task Update_WithExistingId_Returns204NoContent()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/departments", new DepartmentInsertDto { Name = "Sales" });
        var created = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();

        var updateDto = new DepartmentUpdateDto { Name = "Sales & Marketing" };
        var response = await _client.PutAsJsonAsync($"/api/departments/{created!.Id}", updateDto);

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingId_Returns404NotFound()
    {
        var updateDto = new DepartmentUpdateDto { Name = "New Name" };
        var response = await _client.PutAsJsonAsync("/api/departments/99999", updateDto);

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region DELETE /api/departments/{id}

    [Fact]
    public async Task Delete_WithExistingId_Returns204NoContent()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/departments", new DepartmentInsertDto { Name = "Temporary" });
        var created = await createResponse.Content.ReadFromJsonAsync<DepartmentDto>();

        var response = await _client.DeleteAsync($"/api/departments/{created!.Id}");

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.DeleteAsync("/api/departments/99999");

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
}