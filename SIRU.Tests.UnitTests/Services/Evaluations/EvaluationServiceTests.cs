using Moq;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Services.Evaluations;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;
using System.Linq.Expressions;

namespace SIRU.Tests.UnitTests.Services.Evaluations;

public class EvaluationServiceTests
{
    private readonly Mock<IGenericRepository<Evaluation>> _evaluationRepoMock;
    private readonly Mock<IGenericRepository<Criterion>> _criterionRepoMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly EvaluationService _service;

    public EvaluationServiceTests()
    {
        _evaluationRepoMock = new Mock<IGenericRepository<Evaluation>>();
        _criterionRepoMock = new Mock<IGenericRepository<Criterion>>();
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _service = new EvaluationService(_evaluationRepoMock.Object, _criterionRepoMock.Object, _employeeRepoMock.Object);
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidData_ReturnsEvaluationDto()
    {
        var employeeId = "emp-1";
        var employee = CreateEmployee(employeeId, "John", "Doe");
        var currentPosition = CreateEmployeePosition(1, employeeId, 1, "Developer", "Engineering");
        currentPosition.Employee = employee;
        var criteria = CreateCriteria((1, "Teamwork"), (2, "Responsibility"));

        _employeeRepoMock.Setup(r => r.GetCurrentPositionAsync(employeeId)).ReturnsAsync(currentPosition);
        _criterionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>())).ReturnsAsync(criteria);
        _evaluationRepoMock.Setup(r => r.AddAsync(It.IsAny<Evaluation>())).Returns(Task.CompletedTask);

        var dto = new EvaluationInsertDto
        {
            EmployeeId = employeeId,
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>
            {
                new() { CriterionId = 1, Score = 4.0f, Observation = "Good" },
                new() { CriterionId = 2, Score = 5.0f, Observation = "Excellent" }
            }
        };

        var result = await _service.AddAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(4.5f, result.Value!.AverageScore);
        Assert.Equal(2, result.Value.Criteria.Count);
    }

    [Fact]
    public async Task AddAsync_WithEmptyCriteria_ReturnsFailure()
    {
        var dto = new EvaluationInsertDto
        {
            EmployeeId = "emp-1",
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>()
        };

        var result = await _service.AddAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("Debe incluir al menos un criterio.", result.Error);
    }

    [Fact]
    public async Task AddAsync_WithNonExistentEmployee_ReturnsFailure()
    {
        _employeeRepoMock.Setup(r => r.GetCurrentPositionAsync(It.IsAny<string>())).ReturnsAsync((EmployeePosition?)null);

        var dto = new EvaluationInsertDto
        {
            EmployeeId = "nonexistent",
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>
            {
                new() { CriterionId = 1, Score = 4.0f }
            }
        };

        var result = await _service.AddAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("Empleado no encontrado o no tiene posición activa.", result.Error);
    }

    [Fact]
    public async Task AddAsync_WithNoActivePosition_ReturnsFailure()
    {
        var employeeId = "emp-1";
        var employee = CreateEmployee(employeeId, "John", "Doe");
        employee.PositionsOccupied = new List<EmployeePosition>
        {
            new EmployeePosition { Id = 1, EmployeeId = employeeId, PositionId = 1, StartDate = DateTime.UtcNow.AddYears(-2), EndDate = DateTime.UtcNow.AddYears(-1) }
        };

        _employeeRepoMock.Setup(r => r.GetCurrentPositionAsync(employeeId)).ReturnsAsync((EmployeePosition?)null);

        var dto = new EvaluationInsertDto
        {
            EmployeeId = employeeId,
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>
            {
                new() { CriterionId = 1, Score = 4.0f }
            }
        };

        var result = await _service.AddAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("Empleado no encontrado o no tiene posición activa.", result.Error);
    }

    [Fact]
    public async Task AddAsync_WithNonExistentCriterion_ReturnsFailure()
    {
        var employeeId = "emp-1";
        var employee = CreateEmployee(employeeId, "John", "Doe");
        var currentPosition = CreateEmployeePosition(1, employeeId, 1, "Developer", "Engineering");
        currentPosition.Employee = employee;

        _employeeRepoMock.Setup(r => r.GetCurrentPositionAsync(employeeId)).ReturnsAsync(currentPosition);
        _criterionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>())).ReturnsAsync(new List<Criterion>());

        var dto = new EvaluationInsertDto
        {
            EmployeeId = employeeId,
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>
            {
                new() { CriterionId = 99, Score = 4.0f }
            }
        };

        var result = await _service.AddAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("Criterio(s) no encontrado(s): 99", result.Error);
    }

    [Fact]
    public async Task AddAsync_AverageScore_IsComputedCorrectly()
    {
        var employeeId = "emp-1";
        var employee = CreateEmployee(employeeId, "John", "Doe");
        var currentPosition = CreateEmployeePosition(1, employeeId, 1, "Developer", "Engineering");
        currentPosition.Employee = employee;
        var criteria = CreateCriteria((1, "Teamwork"), (2, "Responsibility"), (3, "Punctuality"));

        _employeeRepoMock.Setup(r => r.GetCurrentPositionAsync(employeeId)).ReturnsAsync(currentPosition);
        _criterionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>())).ReturnsAsync(criteria);
        _evaluationRepoMock.Setup(r => r.AddAsync(It.IsAny<Evaluation>())).Returns(Task.CompletedTask);

        var dto = new EvaluationInsertDto
        {
            EmployeeId = employeeId,
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>
            {
                new() { CriterionId = 1, Score = 2.0f },
                new() { CriterionId = 2, Score = 4.0f },
                new() { CriterionId = 3, Score = 5.0f }
            }
        };

        var result = await _service.AddAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(3.67f, result.Value!.AverageScore);
    }

    [Fact]
    public async Task AddAsync_WithSingleCriterion_ReturnsCorrectAverage()
    {
        var employeeId = "emp-1";
        var employee = CreateEmployee(employeeId, "John", "Doe");
        var currentPosition = CreateEmployeePosition(1, employeeId, 1, "Developer", "Engineering");
        currentPosition.Employee = employee;
        var criteria = CreateCriteria((1, "Teamwork"));

        _employeeRepoMock.Setup(r => r.GetCurrentPositionAsync(employeeId)).ReturnsAsync(currentPosition);
        _criterionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Criterion, bool>>>())).ReturnsAsync(criteria);
        _evaluationRepoMock.Setup(r => r.AddAsync(It.IsAny<Evaluation>())).Returns(Task.CompletedTask);

        var dto = new EvaluationInsertDto
        {
            EmployeeId = employeeId,
            EvaluationDate = DateTime.UtcNow,
            Criteria = new List<EvaluationCriterionInsertDto>
            {
                new() { CriterionId = 1, Score = 5.0f, Observation = "Perfect" }
            }
        };

        var result = await _service.AddAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(5.0f, result.Value!.AverageScore);
    }

    #endregion

    #region Helper Methods

    private static Employee CreateEmployee(string id, string firstName, string lastName)
    {
        return new Employee
        {
            Id = id,
            Names = firstName,
            LastNames = lastName,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
            PhoneNumber = "555-0000",
            Address = "Test Address",
            IdCard = $"ID-{id}",
            Birthdate = new DateTime(1990, 1, 1),
            Status = true
        };
    }

    private static EmployeePosition CreateEmployeePosition(int id, string employeeId, int positionId, string positionName, string departmentName)
    {
        var department = new Department { Id = 1, Name = departmentName };
        var position = new Position { Id = positionId, Name = positionName, DepartmentId = 1, Department = department, Salary = 50000m };
        return new EmployeePosition
        {
            Id = id,
            EmployeeId = employeeId,
            PositionId = positionId,
            Position = position,
            StartDate = DateTime.UtcNow.AddYears(-1),
            EndDate = null
        };
    }

    private static List<Criterion> CreateCriteria(params (int Id, string Name)[] pairs)
    {
        return pairs.Select(p => new Criterion { Id = p.Id, Name = p.Name }).ToList();
    }

    #endregion
}