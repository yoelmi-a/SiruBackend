using Moq;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Mappings;
using SIRU.Core.Application.Services.Evaluations;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using System.Linq.Expressions;

namespace SIRU.Tests.UnitTests.Services.Evaluations
{
    public class CriterionServiceTests
    {
        private readonly Mock<IGenericRepository<Criterion>> _repositoryMock;
        private readonly CriterionService _service;

        public CriterionServiceTests()
        {
            MappingConfig.RegisterMappings();
            _repositoryMock = new Mock<IGenericRepository<Criterion>>();
            _service = new CriterionService(_repositoryMock.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            var criteria = new List<Criterion> { CreateCriterion(1, "Teamwork"), CreateCriterion(2, "Responsibility") };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(criteria);

            var result = await _service.GetAllAsync();

            Assert.Equal(criteria.Count, result.Count());
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenFound_ShouldReturnSuccess()
        {
            var id = 1;
            var criterion = CreateCriterion(id, "Teamwork");

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(criterion);

            var result = await _service.GetByIdAsync(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(id, result.Value!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ShouldReturnFailure()
        {
            var id = 99;

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Criterion?)null);

            var result = await _service.GetByIdAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Errors);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ShouldAddAndReturnSuccess()
        {
            var insertDto = new CriterionInsertDto { Name = "Teamwork" };

            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>())).ReturnsAsync(new List<Criterion>());
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Criterion>())).Returns(Task.CompletedTask);

            var result = await _service.AddAsync(insertDto);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Criterion>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WithDuplicateName_ReturnsFailure()
        {
            var insertDto = new CriterionInsertDto { Name = "Teamwork" };
            var existing = CreateCriterion(1, "Teamwork");

            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>())).ReturnsAsync(new List<Criterion> { existing });

            var result = await _service.AddAsync(insertDto);

            Assert.False(result.IsSuccess);
            Assert.Contains("El nombre del criterio ya está registrado.", result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenFound_ShouldUpdateAndReturnSuccess()
        {
            var id = 1;
            var existingCriterion = CreateCriterion(id, "Teamwork");
            var updateDto = new CriterionUpdateDto { Name = "Updated Teamwork" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingCriterion);
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>())).ReturnsAsync(new List<Criterion>());
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Criterion>())).ReturnsAsync(existingCriterion);

            var result = await _service.UpdateAsync(id, updateDto);

            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Criterion>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ShouldReturnFailure()
        {
            var id = 99;
            var updateDto = new CriterionUpdateDto { Name = "Updated" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Criterion?)null);

            var result = await _service.UpdateAsync(id, updateDto);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Errors);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateName_ReturnsFailure()
        {
            var id = 1;
            var existingCriterion = CreateCriterion(id, "Teamwork");
            var conflictingCriterion = CreateCriterion(2, "Teamwork");
            var updateDto = new CriterionUpdateDto { Name = "Teamwork" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingCriterion);
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>())).ReturnsAsync(new List<Criterion> { conflictingCriterion });

            var result = await _service.UpdateAsync(id, updateDto);

            Assert.False(result.IsSuccess);
            Assert.Contains("El nombre del criterio ya está registrado.", result.Errors);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenFoundAndNotInUse_ReturnsSuccess()
        {
            var id = 1;
            var criterion = CreateCriterion(id, "Teamwork");

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(criterion);
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>())).ReturnsAsync(new List<Criterion>());
            _repositoryMock.Setup(r => r.RemoveAsync(It.IsAny<Criterion>())).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync(id);

            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.RemoveAsync(It.IsAny<Criterion>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFailure()
        {
            var id = 99;

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Criterion?)null);

            var result = await _service.DeleteAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Errors);
        }

        [Fact]
        public async Task DeleteAsync_WhenCriterionInUse_ReturnsFailure()
        {
            var id = 1;
            var criterion = CreateCriterion(id, "Teamwork");
            var evaluationCriterion = new EvaluationCriterion { EvaluationId = "eval-1", CriteriaId = id, Score = 4.0f };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(criterion);
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>()))
                .ReturnsAsync(new List<Criterion> { criterion });

            var result = await _service.DeleteAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Contains("No se puede eliminar el criterio porque está siendo utilizado en al menos una evaluación.", result.Errors);
        }

        #endregion

        #region Helper Methods

        private static Criterion CreateCriterion(int id, string name)
        {
            return new Criterion { Id = id, Name = name };
        }

        #endregion
    }
}