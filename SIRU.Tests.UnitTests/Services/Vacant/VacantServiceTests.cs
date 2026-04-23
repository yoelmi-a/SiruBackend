using Moq;
using SIRU.Core.Application.Dtos.Vacants;
using SIRU.Core.Application.Dtos.Vacancies;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Application.Mappings;
using SIRU.Core.Application.Services.Vacants;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using SIRUVacant = SIRU.Core.Domain.Entities.Vacant;
using SIRUEnums = SIRU.Core.Domain.Common.Enums;

namespace SIRU.Tests.UnitTests.Services.Vacant
{
    public class VacantServiceTests
    {
        private readonly Mock<IGenericRepository<SIRUVacant>> _repositoryMock;
        private readonly Mock<ICandidateRepository> _candidateRepositoryMock;
        private readonly Mock<IVacancyCandidateRepository> _vacancyCandidateRepositoryMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly Mock<IRankingQueue> _rankingQueueMock;
        private readonly VacantService _service;

        public VacantServiceTests()
        {
            MappingConfig.RegisterMappings();
            _repositoryMock = new Mock<IGenericRepository<SIRUVacant>>();
            _candidateRepositoryMock = new Mock<ICandidateRepository>();
            _vacancyCandidateRepositoryMock = new Mock<IVacancyCandidateRepository>();
            _fileStorageMock = new Mock<IFileStorageService>();
            _rankingQueueMock = new Mock<IRankingQueue>();
            _service = new VacantService(
                _repositoryMock.Object,
                _candidateRepositoryMock.Object,
                _vacancyCandidateRepositoryMock.Object,
                _fileStorageMock.Object,
                _rankingQueueMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var entities = new List<SIRUVacant> { CreateVacant("1", "Dev") };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

            var result = await _service.GetAllAsync();

            Assert.Equal(entities.Count, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_ShouldReturnSuccess()
        {
            var id = "1";
            var entity = CreateVacant(id, "Dev");

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

            var result = await _service.GetByIdAsync(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(id, result.Value!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
        {
            var id = "99";
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((SIRUVacant?)null);

            var result = await _service.GetByIdAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Error);
        }

        [Fact]
        public async Task AddAsync_ShouldAddAndReturnSuccess()
        {
            var saveDto = new SaveVacantDto { Title = "New Dev", Description = "Desc", Profile = "Profile", PositionId = 1 };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SIRUVacant>())).Returns(Task.CompletedTask);

            var result = await _service.AddAsync(saveDto);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SIRUVacant>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenFound_ShouldUpdateAndReturnSuccess()
        {
            var id = "1";
            var updateDto = new UpdateVacantDto { Title = "Updated", Description = "Desc", Profile = "Profile", Status = "Open" };
            var entity = CreateVacant(id, "Old");

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<SIRUVacant>())).ReturnsAsync(entity);

            var result = await _service.UpdateAsync(id, updateDto);

            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SIRUVacant>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ShouldReturnFailure()
        {
            var id = "99";
            var updateDto = new UpdateVacantDto { Title = "Updated", Description = "Desc", Profile = "Profile", Status = "Open" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((SIRUVacant?)null);

            var result = await _service.UpdateAsync(id, updateDto);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Error);
        }

        [Fact]
        public async Task DeleteAsync_WhenFound_ShouldRemoveAndReturnSuccess()
        {
            var id = "1";
            var entity = CreateVacant(id, "Dev");
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
            _repositoryMock.Setup(r => r.RemoveAsync(It.IsAny<SIRUVacant>())).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync(id);

            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.RemoveAsync(It.IsAny<SIRUVacant>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ShouldReturnFailure()
        {
            var id = "99";
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((SIRUVacant?)null);

            var result = await _service.DeleteAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Error);
        }

        #region Helpers

        private static SIRUVacant CreateVacant(string id, string title)
        {
            return new SIRUVacant
            {
                Id = id,
                Title = title,
                Description = "Test Description",
                Profile = "Test Profile",
                PublicationDate = DateTime.UtcNow,
                Status = SIRUEnums.VacantStatus.Open,
                PositionId = 1
            };
        }

        private static Mock<IFormFile> CreateMockFormFile(string fileName, long length, string contentType = "application/pdf")
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(length);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            return fileMock;
        }

        private static VacancyApplicationDto CreateValidDto(string email)
        {
            var fileMock = CreateMockFormFile("cv.pdf", 1024);
            return new VacancyApplicationDto
            {
                CandidateNames = "Test",
                CandidateLastNames = "User",
                CandidateEmail = email,
                CandidatePhoneNumber = "123456",
                CvFile = fileMock.Object
            };
        }

        #endregion

        #region ApplyToVacancyAsync Tests

        [Fact]
        public async Task ApplyToVacancyAsync_WithNonExistentVacancy_ReturnsNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync("nonexistent")).ReturnsAsync((SIRUVacant?)null);

            var dto = CreateValidDto("test@test.com");
            var result = await _service.ApplyToVacancyAsync("nonexistent", dto);

            Assert.False(result.IsSuccess);
            var error = result.Error.FirstOrDefault() ?? string.Empty;
            Assert.Equal("Vacancy not found.", error);
        }

        [Fact]
        public async Task ApplyToVacancyAsync_WithClosedVacancy_ReturnsConflict()
        {
            var closedVacant = CreateVacant("v1", "Dev");
            closedVacant.Status = SIRUEnums.VacantStatus.Closed;
            _repositoryMock.Setup(r => r.GetByIdAsync("v1")).ReturnsAsync(closedVacant);

            var dto = CreateValidDto("test@test.com");
            var result = await _service.ApplyToVacancyAsync("v1", dto);

            Assert.False(result.IsSuccess);
            var error = result.Error.FirstOrDefault() ?? string.Empty;
            Assert.Equal("Vacancy is not open for applications.", error);
        }

        [Fact]
        public async Task ApplyToVacancyAsync_WithNonPdfFile_ReturnsBadRequest()
        {
            var openVacant = CreateVacant("v1", "Dev");
            openVacant.Status = SIRUEnums.VacantStatus.Open;
            _repositoryMock.Setup(r => r.GetByIdAsync("v1")).ReturnsAsync(openVacant);

            var fileMock = CreateMockFormFile("cv.doc", 1024, "application/msword");
            _fileStorageMock.Setup(f => f.GetContentType("cv.doc")).Returns("application/msword");

            var dto = new VacancyApplicationDto
            {
                CandidateNames = "Test",
                CandidateLastNames = "User",
                CandidateEmail = "test@test.com",
                CandidatePhoneNumber = "123456",
                CvFile = fileMock.Object
            };

            var result = await _service.ApplyToVacancyAsync("v1", dto);

            Assert.False(result.IsSuccess);
            var error = result.Error.FirstOrDefault() ?? string.Empty;
            Assert.Equal("Only PDF files are accepted.", error);
        }

        [Fact]
        public async Task ApplyToVacancyAsync_WithOversizedFile_ReturnsBadRequest()
        {
            var openVacant = CreateVacant("v1", "Dev");
            openVacant.Status = SIRUEnums.VacantStatus.Open;
            _repositoryMock.Setup(r => r.GetByIdAsync("v1")).ReturnsAsync(openVacant);

            var largeFileMock = CreateMockFormFile("cv.pdf", 11 * 1024 * 1024);
            _fileStorageMock.Setup(f => f.GetContentType("cv.pdf")).Returns("application/pdf");

            var dto = new VacancyApplicationDto
            {
                CandidateNames = "Test",
                CandidateLastNames = "User",
                CandidateEmail = "test@test.com",
                CandidatePhoneNumber = "123456",
                CvFile = largeFileMock.Object
            };

            var result = await _service.ApplyToVacancyAsync("v1", dto);

            Assert.False(result.IsSuccess);
            var error = result.Error.FirstOrDefault() ?? string.Empty;
            Assert.Equal("File size must not exceed 10 MB.", error);
        }

        [Fact]
        public async Task ApplyToVacancyAsync_WithExistingCandidate_DoesNotCreateCandidate()
        {
            var openVacant = CreateVacant("v1", "Dev");
            openVacant.Status = SIRUEnums.VacantStatus.Open;
            _repositoryMock.Setup(r => r.GetByIdAsync("v1")).ReturnsAsync(openVacant);

            var existingCandidate = new Candidate
            {
                Id = "c1",
                Names = "John",
                LastNames = "Doe",
                Email = "john@test.com",
                PhoneNumber = "123456"
            };
            _candidateRepositoryMock.Setup(r => r.FindByEmailAsync("john@test.com")).ReturnsAsync(existingCandidate);
            _fileStorageMock.Setup(f => f.GetContentType(It.IsAny<string>())).Returns("application/pdf");
            _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "cvs")).ReturnsAsync("/path/cv.pdf");

            var dto = CreateValidDto("john@test.com");
            var result = await _service.ApplyToVacancyAsync("v1", dto);

            Assert.True(result.IsSuccess);
            _candidateRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Candidate>()), Times.Never);
            _vacancyCandidateRepositoryMock.Verify(r => r.AddAsync(It.IsAny<VacancyCandidate>()), Times.Once);
        }

        [Fact]
        public async Task ApplyToVacancyAsync_WithNewCandidate_CreatesCandidateAndApplication()
        {
            var openVacant = CreateVacant("v1", "Dev");
            openVacant.Status = SIRUEnums.VacantStatus.Open;
            _repositoryMock.Setup(r => r.GetByIdAsync("v1")).ReturnsAsync(openVacant);

            _candidateRepositoryMock.Setup(r => r.FindByEmailAsync("new@test.com")).ReturnsAsync((Candidate?)null);
            _fileStorageMock.Setup(f => f.GetContentType(It.IsAny<string>())).Returns("application/pdf");
            _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "cvs")).ReturnsAsync("/path/cv.pdf");

            var dto = CreateValidDto("new@test.com");
            var result = await _service.ApplyToVacancyAsync("v1", dto);

            Assert.True(result.IsSuccess);
            _candidateRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Candidate>()), Times.Once);
            _vacancyCandidateRepositoryMock.Verify(r => r.AddAsync(It.IsAny<VacancyCandidate>()), Times.Once);
        }

        [Fact]
        public async Task ApplyToVacancyAsync_WithValidData_ReturnsCreatedWithScoreZero()
        {
            var openVacant = CreateVacant("v1", "Dev");
            openVacant.Status = SIRUEnums.VacantStatus.Open;
            _repositoryMock.Setup(r => r.GetByIdAsync("v1")).ReturnsAsync(openVacant);

            var existingCandidate = new Candidate
            {
                Id = "c1",
                Names = "John",
                LastNames = "Doe",
                Email = "john@test.com",
                PhoneNumber = "123456"
            };
            _candidateRepositoryMock.Setup(r => r.FindByEmailAsync("john@test.com")).ReturnsAsync(existingCandidate);
            _fileStorageMock.Setup(f => f.GetContentType(It.IsAny<string>())).Returns("application/pdf");
            _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "cvs")).ReturnsAsync("/path/cv.pdf");

            var dto = CreateValidDto("john@test.com");
            var result = await _service.ApplyToVacancyAsync("v1", dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(0.0f, result.Value!.Score);
            Assert.Equal(SIRUEnums.CandidateStatus.Pending, result.Value.Status);
            Assert.Equal("/path/cv.pdf", result.Value.CvUrl);
            _rankingQueueMock.Verify(q => q.EnqueueAsync(It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region RecalculateScoresAsync Tests

        [Fact]
        public async Task RecalculateScoresAsync_WithExistingVacancy_EnqueuesAllCandidatesAndReturnsSuccess()
        {
            var vacantId = "v1";
            var vacant = CreateVacant(vacantId, "Dev");
            _repositoryMock.Setup(r => r.GetByIdAsync(vacantId)).ReturnsAsync(vacant);

            var candidates = new List<VacancyCandidate>
            {
                new() { Id = "app1", VacantId = vacantId, CandidateId = "c1", Score = 0.5f, Status = SIRUEnums.CandidateStatus.Pending, CvUrl = "/cv1.pdf" },
                new() { Id = "app2", VacantId = vacantId, CandidateId = "c2", Score = 0.8f, Status = SIRUEnums.CandidateStatus.Pending, CvUrl = "/cv2.pdf" }
            };
            _vacancyCandidateRepositoryMock.Setup(r => r.GetAllByVacancyIdAsync(vacantId)).ReturnsAsync(candidates);

            var result = await _service.RecalculateScoresAsync(vacantId);

            Assert.True(result.IsSuccess);
            _rankingQueueMock.Verify(q => q.EnqueueAsync("app1"), Times.Once);
            _rankingQueueMock.Verify(q => q.EnqueueAsync("app2"), Times.Once);
        }

        [Fact]
        public async Task RecalculateScoresAsync_WithNonExistentVacancy_ReturnsNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync("nonexistent")).ReturnsAsync((SIRUVacant?)null);

            var result = await _service.RecalculateScoresAsync("nonexistent");

            Assert.False(result.IsSuccess);
            var error = result.Error.FirstOrDefault() ?? string.Empty;
            Assert.Equal("Vacancy not found.", error);
        }

        #endregion

        #region GetApplicationsByVacancyAsync Tests

        [Fact]
        public async Task GetApplicationsByVacancyAsync_WithExistingVacancy_ReturnsPaginatedList()
        {
            var vacantId = "v1";
            var vacant = CreateVacant(vacantId, "Dev");
            _repositoryMock.Setup(r => r.GetByIdAsync(vacantId)).ReturnsAsync(vacant);

            var candidates = new List<VacancyCandidate>
            {
                new() { Id = "app1", VacantId = vacantId, CandidateId = "c1", Score = 0.8f, Status = SIRUEnums.CandidateStatus.Pending, CvUrl = "/cv1.pdf", Candidate = new Candidate { Id = "c1", Names = "John", LastNames = "Doe", Email = "john@test.com", PhoneNumber = "123" } },
                new() { Id = "app2", VacantId = vacantId, CandidateId = "c2", Score = 0.5f, Status = SIRUEnums.CandidateStatus.Pending, CvUrl = "/cv2.pdf", Candidate = new Candidate { Id = "c2", Names = "Jane", LastNames = "Smith", Email = "jane@test.com", PhoneNumber = "456" } }
            };

            var pagination = new SIRU.Core.Domain.Common.Pagination.Pagination(1, 10);
            var paginatedResponse = new SIRU.Core.Domain.Common.Pagination.PaginatedResponse<VacancyCandidate>
            {
                Items = candidates,
                Pagination = new SIRU.Core.Domain.Common.Pagination.Pagination { PageNumber = 1, PageSize = 10, TotalCount = 2 }
            };
            _vacancyCandidateRepositoryMock.Setup(r => r.GetPaginatedByVacancyIdAsync(vacantId, It.IsAny<SIRU.Core.Domain.Common.Pagination.Pagination>())).ReturnsAsync(paginatedResponse);

            var result = await _service.GetApplicationsByVacancyAsync(vacantId, pagination);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.Items.Count());
            Assert.Equal("John Doe", result.Value.Items.First().CandidateFullName);
            Assert.Equal(0.8f, result.Value.Items.First().Score);
        }

        [Fact]
        public async Task GetApplicationsByVacancyAsync_WithNonExistentVacancy_ReturnsNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync("nonexistent")).ReturnsAsync((SIRUVacant?)null);

            var pagination = new SIRU.Core.Domain.Common.Pagination.Pagination(1, 10);
            var result = await _service.GetApplicationsByVacancyAsync("nonexistent", pagination);

            Assert.False(result.IsSuccess);
            var error = result.Error.FirstOrDefault() ?? string.Empty;
            Assert.Equal("Vacancy not found.", error);
        }

        #endregion
    }
}