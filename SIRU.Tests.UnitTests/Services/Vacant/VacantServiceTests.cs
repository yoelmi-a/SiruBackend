using Moq;
using SIRU.Core.Application.Dtos.Vacants;
using SIRU.Core.Application.Mappings;
using SIRU.Core.Application.Services.Vacants;
using SIRU.Core.Domain.Interfaces;
using SIRUVacant = SIRU.Core.Domain.Entities.Vacant;
using SIRUEnums = SIRU.Core.Domain.Common.Enums;

namespace SIRU.Tests.UnitTests.Services.Vacant
{
    public class VacantServiceTests
    {
        private readonly Mock<IGenericRepository<SIRUVacant>> _repositoryMock;
        private readonly VacantService _service;

        public VacantServiceTests()
        {
            MappingConfig.RegisterMappings();
            _repositoryMock = new Mock<IGenericRepository<SIRUVacant>>();
            _service = new VacantService(_repositoryMock.Object);
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
    }
}