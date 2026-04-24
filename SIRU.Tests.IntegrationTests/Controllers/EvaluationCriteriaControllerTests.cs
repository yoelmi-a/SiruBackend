namespace SIRU.Tests.IntegrationTests.Controllers;

public class EvaluationCriteriaControllerTests : IntegrationTestBase
{
    private HttpClient _client = null!;

    public EvaluationCriteriaControllerTests()
    {
        _client = CreateDefaultClient();
    }

    #region GET /api/evaluation-criteria

    [Fact]
    public async Task GetAll_WithNoCriteria_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/evaluation-criteria");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var criteria = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(criteria);
        Assert.Empty(criteria);
    }

    #endregion

    #region POST /api/evaluation-criteria

    [Fact]
    public async Task Create_WithValidData_Returns201Created()
    {
        var dto = new { name = "Teamwork" };
        var response = await _client.PostAsJsonAsync("/api/evaluation-criteria", dto);

        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CriterionResponse>();
        Assert.NotNull(result);
        Assert.Equal("Teamwork", result.Name);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns400BadRequest()
    {
        var dto = new { name = "" };
        var response = await _client.PostAsJsonAsync("/api/evaluation-criteria", dto);

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Create_WithDuplicateName_Returns409Conflict()
    {
        var dto = new { name = "Leadership" };
        await _client.PostAsJsonAsync("/api/evaluation-criteria", dto);

        var duplicateResponse = await _client.PostAsJsonAsync("/api/evaluation-criteria", dto);

        Assert.Equal(StatusCodes.Status409Conflict, (int)duplicateResponse.StatusCode);
    }

    #endregion

    #region GET /api/evaluation-criteria/{id}

    [Fact]
    public async Task GetById_WithExistingId_Returns200Ok()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/evaluation-criteria", new { name = "Communication" });
        var created = await createResponse.Content.ReadFromJsonAsync<CriterionResponse>();

        var response = await _client.GetAsync($"/api/evaluation-criteria/{created!.Id}");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CriterionResponse>();
        Assert.NotNull(result);
        Assert.Equal("Communication", result.Name);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/evaluation-criteria/99999");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region PUT /api/evaluation-criteria/{id}

    [Fact]
    public async Task Update_WithExistingId_Returns200Ok()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/evaluation-criteria", new { name = "Problem Solving" });
        var created = await createResponse.Content.ReadFromJsonAsync<CriterionResponse>();

        var updateDto = new { name = "Problem Solving Updated" };
        var response = await _client.PutAsJsonAsync($"/api/evaluation-criteria/{created!.Id}", updateDto);

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CriterionResponse>();
        Assert.NotNull(result);
        Assert.Equal("Problem Solving Updated", result.Name);
    }

    [Fact]
    public async Task Update_WithNonExistingId_Returns404NotFound()
    {
        var updateDto = new { name = "New Name" };
        var response = await _client.PutAsJsonAsync("/api/evaluation-criteria/99999", updateDto);

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region DELETE /api/evaluation-criteria/{id}

    [Fact]
    public async Task Delete_WithExistingId_Returns204NoContent()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/evaluation-criteria", new { name = "To Delete" });
        var created = await createResponse.Content.ReadFromJsonAsync<CriterionResponse>();

        var response = await _client.DeleteAsync($"/api/evaluation-criteria/{created!.Id}");

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.DeleteAsync("/api/evaluation-criteria/99999");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    private class CriterionResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}