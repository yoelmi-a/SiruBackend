using System.Text.Json.Serialization;
using SIRU.Core.Application.Dtos.Candidates;
using System.Net;
using System.Net.Http.Json;

namespace SIRU.Tests.IntegrationTests.Controllers;

public class CandidatesControllerTests : IntegrationTestBase
{
    private HttpClient _client = null!;

    public CandidatesControllerTests()
    {
        _client = CreateDefaultClient();
    }

    #region GET /api/candidates

    [Fact]
    public async Task GetAll_WithNoCandidates_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/candidates");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var candidates = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(candidates);
        Assert.Empty(candidates);
    }

    #endregion

    #region POST /api/candidates

    [Fact]
    public async Task Create_WithValidData_Returns201Created()
    {
        var dto = new CandidateInsertDto
        {
            Names = "John",
            LastNames = "Doe",
            Email = "john.doe@test.com",
            PhoneNumber = "1234567890"
        };
        var response = await _client.PostAsJsonAsync("/api/candidates", dto);

        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CandidateDto>();
        Assert.NotNull(result);
        Assert.Equal("John", result.Names);
        Assert.Equal("Doe", result.LastNames);
    }

    [Fact]
    public async Task Create_WithMissingEmail_Returns400BadRequest()
    {
        var dto = new CandidateInsertDto
        {
            Names = null!,
            LastNames = "Smith",
            Email = "jane@test.com",
            PhoneNumber = "5551234"
        };
        var response = await _client.PostAsJsonAsync("/api/candidates", dto);

        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    #endregion

    #region GET /api/candidates/{id}

    [Fact]
    public async Task GetById_WithExistingId_Returns200Ok()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/candidates", new CandidateInsertDto
        {
            Names = "Alice",
            LastNames = "Brown",
            Email = "alice@test.com",
            PhoneNumber = "111222333"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CandidateDto>();

        var response = await _client.GetAsync($"/api/candidates/{created!.Id}");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CandidateDto>();
        Assert.NotNull(result);
        Assert.Equal("Alice", result.Names);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/candidates/nonexistent-id");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region PUT /api/candidates/{id}

    [Fact]
    public async Task Update_WithExistingId_Returns204NoContent()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/candidates", new CandidateInsertDto
        {
            Names = "Bob",
            LastNames = "Wilson",
            Email = "bob@test.com",
            PhoneNumber = "999888777"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CandidateDto>();

        var updateDto = new CandidateUpdateDto
        {
            Id = created!.Id,
            Names = "Robert",
            LastNames = "Wilson",
            Email = "robert@test.com",
            PhoneNumber = "999888777"
        };
        var response = await _client.PutAsJsonAsync($"/api/candidates/{created.Id}", updateDto);

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingId_Returns404NotFound()
    {
        var updateDto = new CandidateUpdateDto
        {
            Id = "nonexistent-id",
            Names = "Updated",
            LastNames = "Name",
            Email = "updated@test.com",
            PhoneNumber = "123"
        };
        var response = await _client.PutAsJsonAsync("/api/candidates/nonexistent-id", updateDto);

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    #region DELETE /api/candidates/{id}

    [Fact]
    public async Task Delete_WithExistingId_Returns204NoContent()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/candidates", new CandidateInsertDto
        {
            Names = "ToDelete",
            LastNames = "Candidate",
            Email = "delete@test.com",
            PhoneNumber = "1234567890"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CandidateDto>();

        var response = await _client.DeleteAsync($"/api/candidates/{created!.Id}");

        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_Returns404NotFound()
    {
        var response = await _client.DeleteAsync("/api/candidates/nonexistent-id");

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    #endregion

    private class CandidateDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [JsonPropertyName("names")]
        public required string Names { get; set; }
        [JsonPropertyName("lastNames")]
        public required string LastNames { get; set; }
        [JsonPropertyName("email")]
        public required string Email { get; set; }
        [JsonPropertyName("phoneNumber")]
        public required string PhoneNumber { get; set; }
        [JsonPropertyName("cvUrl")]
        public required string CvUrl { get; set; }
    }
}