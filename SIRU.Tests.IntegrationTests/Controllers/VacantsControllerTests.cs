namespace SIRU.Tests.IntegrationTests.Controllers;

public class VacantsControllerTests : IntegrationTestBase
{
    private HttpClient _client = null!;

    public VacantsControllerTests()
    {
        _client = CreateDefaultClient();
    }

    private async Task<(int departmentId, int positionId)> SeedDepartmentAndPositionAsync()
    {
        var deptResponse = await _client.PostAsJsonAsync("/api/departments", new { name = "Engineering" });
        var dept = await deptResponse.Content.ReadFromJsonAsync<DepartmentResponse>();

        var posResponse = await _client.PostAsJsonAsync("/api/positions", new { name = "Software Engineer", salary = 60000m, departmentId = dept!.Id });
        var pos = await posResponse.Content.ReadFromJsonAsync<PositionResponse>();

        return (dept.Id, pos!.Id);
    }

    #region GET /api/vacants

    [Fact]
    public async Task GetAll_WithNoVacants_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/vacants");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var vacants = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(vacants);
        Assert.Empty(vacants);
    }

    #endregion

    #region POST /api/vacants

    [Fact]
    public async Task Create_WithValidData_Returns201Created()
    {
        var (_, positionId) = await SeedDepartmentAndPositionAsync();

        var dto = new { title = "Backend Developer", description = "Build APIs", profile = "3+ years experience", positionId };
        var response = await _client.PostAsJsonAsync("/api/vacants", dto);

        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<VacantResponse>();
        Assert.NotNull(result);
        Assert.Equal("Backend Developer", result.Title);
        Assert.Equal("Open", result.Status);
    }

    [Fact]
    public async Task Create_WithMissingFields_Returns400BadRequest()
    {
        var dto = new { title = "Only Title" };
        var response = await _client.PostAsJsonAsync("/api/vacants", dto);

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    #endregion

    #region GET /api/vacants/{id}

    [Fact]
    public async Task GetById_WithExistingId_Returns200Ok()
    {
        var (_, positionId) = await SeedDepartmentAndPositionAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/vacants", new { title = "Frontend Dev", description = "React developer", profile = "2 years", positionId });
        var created = await createResponse.Content.ReadFromJsonAsync<VacantResponse>();

        var response = await _client.GetAsync($"/api/vacants/{created!.Id}");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<VacantResponse>();
        Assert.NotNull(result);
        Assert.Equal("Frontend Dev", result.Title);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/vacants/nonexistent-id");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region PUT /api/vacants/{id}

    [Fact]
    public async Task Update_WithExistingId_Returns204NoContent()
    {
        var (_, positionId) = await SeedDepartmentAndPositionAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/vacants", new { title = "Old Title", description = "Old desc", profile = "Old profile", positionId });
        var created = await createResponse.Content.ReadFromJsonAsync<VacantResponse>();

        var updateDto = new { title = "Updated Title", description = "Updated desc", profile = "Updated profile", status = "Open", positionId };
        var response = await _client.PutAsJsonAsync($"/api/vacants/{created!.Id}", updateDto);

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingId_Returns404NotFound()
    {
        var (_, positionId) = await SeedDepartmentAndPositionAsync();
        var updateDto = new { title = "Any", description = "Any", profile = "Any", status = "Open", positionId };

        var response = await _client.PutAsJsonAsync("/api/vacants/nonexistent-id", updateDto);

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region DELETE /api/vacants/{id}

    [Fact]
    public async Task Delete_WithExistingId_Returns204NoContent()
    {
        var (_, positionId) = await SeedDepartmentAndPositionAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/vacants", new { title = "To Delete", description = "Temp vacant", profile = "Any", positionId });
        var created = await createResponse.Content.ReadFromJsonAsync<VacantResponse>();

        var response = await _client.DeleteAsync($"/api/vacants/{created!.Id}");

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    #endregion

    #region POST /api/vacants/{vacancyId}/applications

    [Fact]
    public async Task ApplyToVacancy_WithValidPdf_Returns201Created()
    {
        var (_, positionId) = await SeedDepartmentAndPositionAsync();
        var vacantResponse = await _client.PostAsJsonAsync("/api/vacants", new { title = "Open Position", description = "Looking for talent", profile = "Experience required", positionId });
        var vacant = await vacantResponse.Content.ReadFromJsonAsync<VacantResponse>();

        var pdfBytes = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4 mock pdf content for testing");

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Candidate Name"), "candidateNames");
        form.Add(new StringContent("Candidate Last Name"), "candidateLastNames");
        form.Add(new StringContent("candidate@test.com"), "candidateEmail");
        form.Add(new StringContent("12345678"), "candidatePhoneNumber");
        var filePart = new ByteArrayContent(pdfBytes);
        filePart.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        form.Add(filePart, "cvFile", "cv.pdf");

        var response = await _client.PostAsync($"/api/vacants/{vacant!.Id}/applications", form);

        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
    }

    [Fact]
    public async Task ApplyToVacancy_WithNonExistentVacancyId_Returns404NotFound()
    {
        var pdfBytes = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4");
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Name"), "candidateNames");
        form.Add(new StringContent("Last"), "candidateLastNames");
        form.Add(new StringContent("test@test.com"), "candidateEmail");
        form.Add(new StringContent("12345678"), "candidatePhoneNumber");
        var filePart = new ByteArrayContent(pdfBytes);
        filePart.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        form.Add(filePart, "cvFile", "cv.pdf");

        var response = await _client.PostAsync("/api/vacants/nonexistent-id/applications", form);

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region POST /api/vacants/{vacancyId}/recalculate-scores

    [Fact]
    public async Task RecalculateScores_WithExistingVacancy_Returns202Accepted()
    {
        var (_, positionId) = await SeedDepartmentAndPositionAsync();
        var vacantResponse = await _client.PostAsJsonAsync("/api/vacants", new { title = "Scoring Test", description = "Test vacancy", profile = "Any", positionId });
        var vacant = await vacantResponse.Content.ReadFromJsonAsync<VacantResponse>();

        var response = await _client.PostAsync($"/api/vacants/{vacant!.Id}/recalculate-scores", null);

        Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
    }

    [Fact]
    public async Task RecalculateScores_WithNonExistingVacancyId_Returns404NotFound()
    {
        var response = await _client.PostAsync("/api/vacants/nonexistent-id/recalculate-scores", null);

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region GET /api/vacants/{vacancyId}/applications

    [Fact]
    public async Task GetApplications_WithExistingVacancy_Returns200Ok()
    {
        var (_, positionId) = await SeedDepartmentAndPositionAsync();
        var vacantResponse = await _client.PostAsJsonAsync("/api/vacants", new { title = "List Applications", description = "Check candidates", profile = "Any", positionId });
        var vacant = await vacantResponse.Content.ReadFromJsonAsync<VacantResponse>();

        var response = await _client.GetAsync($"/api/vacants/{vacant!.Id}/applications");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApplicationListResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task GetApplications_WithNonExistingVacancyId_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/vacants/nonexistent-id/applications");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    private class DepartmentResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    private class PositionResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Salary { get; set; }
        public int DepartmentId { get; set; }
    }

    private class VacantResponse
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required string Status { get; set; }
    }

    private class ApplicationListResponse
    {
        public required List<object> Items { get; set; }
        public required PaginationResponse Pagination { get; set; }
    }

    private class PaginationResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}