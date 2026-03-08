using AutoMapper;
using Moq;
using SIRU.Core.Application.Dtos.Vacant;
using SIRU.Core.Application.Services.Vacant;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using System.Linq.Expressions;

namespace SIRU.Tests.UnitTests.Services.Vacant
{
    public class VacantServiceTests
    {
        private readonly Mock<IGenericRepository<SIRU.Core.Domain.Entities.Vacant>> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly VacantService _service;

        public VacantServiceTests()
        {
            _repositoryMock = new Mock<IGenericRepository<SIRU.Core.Domain.Entities.Vacant>>();
            _mapperMock = new Mock<IMapper>();
            _service = new VacantService(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnSuccessWithList()
        {
            // Arrange
            var entities = new List<SIRU.Core.Domain.Entities.Vacant> { new() { Id = "1", Title = "Dev", Description = "Desc", Profile = "Profile", PublicationDate = DateTime.UtcNow, Status = 1 } };
            var dtos = new List<VacantDto> { new() { Id = "1", Title = "Dev", Description = "Desc", Profile = "Profile", PublicationDate = DateTime.UtcNow, Status = 1 } };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);
            _mapperMock.Setup(m => m.Map<IEnumerable<VacantDto>>(entities)).Returns(dtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(dtos.Count, result.Value!.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_ShouldReturnSuccess()
        {
            // Arrange
            var id = "1";
            var entity = new SIRU.Core.Domain.Entities.Vacant { Id = id, Title = "Dev", Description = "Desc", Profile = "Profile", PublicationDate = DateTime.UtcNow, Status = 1 };
            var dto = new VacantDto { Id = id, Title = "Dev", Description = "Desc", Profile = "Profile", PublicationDate = DateTime.UtcNow, Status = 1 };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<VacantDto>(entity)).Returns(dto);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(id, result.Value!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
        {
            // Arrange
            var id = "99";
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((SIRU.Core.Domain.Entities.Vacant?)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Vacant not found", result.Error);
        }

        [Fact]
        public async Task AddAsync_ShouldAddAndReturnSuccess()
        {
            // Arrange
            var saveDto = new SaveVacantDto { Title = "New Dev", Description = "Desc", Profile = "Profile", Status = 1 };
            var entity = new SIRU.Core.Domain.Entities.Vacant { Id = "new-id", Title = "New Dev", Description = "Desc", Profile = "Profile", PublicationDate = DateTime.UtcNow, Status = 1 };
            var resultDto = new VacantDto { Id = "new-id", Title = "New Dev", Description = "Desc", Profile = "Profile", PublicationDate = DateTime.UtcNow, Status = 1 };

            _mapperMock.Setup(m => m.Map<SIRU.Core.Domain.Entities.Vacant>(saveDto)).Returns(entity);
            _repositoryMock.Setup(r => r.AddAsync(entity)).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<VacantDto>(entity)).Returns(resultDto);

            // Act
            var result = await _service.AddAsync(saveDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<SIRU.Core.Domain.Entities.Vacant>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenFound_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var id = "1";
            var saveDto = new SaveVacantDto { Title = "Updated", Description = "Desc", Profile = "Profile", Status = 1 };
            var entity = new SIRU.Core.Domain.Entities.Vacant { Id = id, Title = "Old", Description = "Desc", Profile = "Profile", PublicationDate = DateTime.UtcNow, Status = 1 };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map(saveDto, entity)).Returns(entity);

            // Act
            var result = await _service.UpdateAsync(id, saveDto);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.Update(entity), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenFound_ShouldRemoveAndReturnSuccess()
        {
            // Arrange
            var id = "1";
            var entity = new SIRU.Core.Domain.Entities.Vacant { Id = id, Title = "Dev", Description = "Desc", Profile = "Profile", PublicationDate = DateTime.UtcNow, Status = 1 };
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.Remove(entity), Times.Once);
        }
    }
}
