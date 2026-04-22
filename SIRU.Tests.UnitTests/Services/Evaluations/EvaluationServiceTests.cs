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

    #region GetByEmployeeIdAsync Tests

    [Fact]
    public async Task GetByEmployeeIdAsync_WhenEmployeeExists_ReturnsEvaluations()
    {
        var employeeId = "emp-1";
        var employee = CreateEmployee(employeeId, "John", "Doe");
        var position = CreatePosition(1, "Developer", "Engineering");
        var ep = CreateEmployeePosition(1, employeeId, 1, "Developer", "Engineering");
        ep.Employee = employee;
        var evaluations = new List<Evaluation>
        {
            new Evaluation { Id = "eval-1", EmployeePositionId = 1, Date = DateTime.UtcNow.AddDays(-10), AverageScore = 4.0f, EmployeePosition = ep, Criteria = new List<EvaluationCriterion>
                {
                    new EvaluationCriterion { EvaluationId = "eval-1", CriteriaId = 1, Score = 4.0f, Criterion = new Criterion { Id = 1, Name = "Teamwork" } }
                }}
        };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _employeeRepoMock.Setup(r => r.GetEmployeeEvaluationsAsync(employeeId)).ReturnsAsync(evaluations);

        var result = await _service.GetByEmployeeIdAsync(employeeId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!);
        Assert.Equal(4.0f, result.Value!.First().AverageScore);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_WhenEmployeeNotFound_ReturnsFailure()
    {
        var employeeId = "nonexistent";

        _employeeRepoMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync((Employee?)null);

        var result = await _service.GetByEmployeeIdAsync(employeeId);

        Assert.False(result.IsSuccess);
        Assert.Contains("Empleado no encontrado.", result.Error);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_WhenNoEvaluations_ReturnsEmptyList()
    {
        var employeeId = "emp-1";
        var employee = CreateEmployee(employeeId, "John", "Doe");

        _employeeRepoMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _employeeRepoMock.Setup(r => r.GetEmployeeEvaluationsAsync(employeeId)).ReturnsAsync(new List<Evaluation>());

        var result = await _service.GetByEmployeeIdAsync(employeeId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_EvaluationsSortedByDateDescending()
    {
        var employeeId = "emp-1";
        var employee = CreateEmployee(employeeId, "John", "Doe");
        var ep = CreateEmployeePosition(1, employeeId, 1, "Developer", "Engineering");
        ep.Employee = employee;
        var older = new Evaluation { Id = "eval-old", EmployeePositionId = 1, Date = DateTime.UtcNow.AddDays(-30), AverageScore = 3.5f, EmployeePosition = ep, Criteria = new List<EvaluationCriterion>() };
        var newer = new Evaluation { Id = "eval-new", EmployeePositionId = 1, Date = DateTime.UtcNow.AddDays(-5), AverageScore = 4.5f, EmployeePosition = ep, Criteria = new List<EvaluationCriterion>() };
        var evaluations = new List<Evaluation> { newer, older };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _employeeRepoMock.Setup(r => r.GetEmployeeEvaluationsAsync(employeeId)).ReturnsAsync(evaluations);

        var result = await _service.GetByEmployeeIdAsync(employeeId);

        Assert.True(result.IsSuccess);
        var list = result.Value!.ToList();
        Assert.Equal("eval-new", list[0].Id);
        Assert.Equal("eval-old", list[1].Id);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_CriteriaIncludeCriterionNames()
    {
        var employeeId = "emp-1";
        var employee = CreateEmployee(employeeId, "John", "Doe");
        var ep = CreateEmployeePosition(1, employeeId, 1, "Developer", "Engineering");
        ep.Employee = employee;
        var criterion1 = new Criterion { Id = 1, Name = "Teamwork" };
        var criterion2 = new Criterion { Id = 2, Name = "Responsibility" };
        var evaluations = new List<Evaluation>
        {
            new Evaluation
            {
                Id = "eval-1",
                EmployeePositionId = 1,
                Date = DateTime.UtcNow,
                AverageScore = 4.5f,
                EmployeePosition = ep,
                Criteria = new List<EvaluationCriterion>
                {
                    new EvaluationCriterion { EvaluationId = "eval-1", CriteriaId = 1, Score = 4.0f, Observation = "Good", Criterion = criterion1 },
                    new EvaluationCriterion { EvaluationId = "eval-1", CriteriaId = 2, Score = 5.0f, Observation = "Excellent", Criterion = criterion2 }
                }
            }
        };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);
        _employeeRepoMock.Setup(r => r.GetEmployeeEvaluationsAsync(employeeId)).ReturnsAsync(evaluations);

        var result = await _service.GetByEmployeeIdAsync(employeeId);

        Assert.True(result.IsSuccess);
        var criteria = result.Value!.First().Criteria;
        Assert.Equal(2, criteria.Count);
        Assert.Equal("Teamwork", criteria[0].Name);
        Assert.Equal("Responsibility", criteria[1].Name);
        Assert.Equal(4.0f, criteria[0].Score);
        Assert.Equal("Good", criteria[0].Observation);
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
        var position = CreatePosition(positionId, positionName, departmentName);
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

    private static Position CreatePosition(int id, string name, string departmentName)
    {
        var department = new Department { Id = 1, Name = departmentName };
        return new Position { Id = id, Name = name, DepartmentId = 1, Department = department, Salary = 50000m };
    }

    private static List<Criterion> CreateCriteria(params (int Id, string Name)[] pairs)
    {
        return pairs.Select(p => new Criterion { Id = p.Id, Name = p.Name }).ToList();
    }

    #endregion
}