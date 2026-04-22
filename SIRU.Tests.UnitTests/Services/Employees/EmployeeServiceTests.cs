using Moq;
using SIRU.Core.Application.Dtos.Employees;
using SIRU.Core.Application.Mappings;
using SIRU.Core.Application.Services.Employees;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Interfaces;
using SIRU.Core.Domain.Entities;
using System.Linq.Expressions;

namespace SIRU.Tests.UnitTests.Services.Employees
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IGenericRepository<Employee>> _repositoryMock;
        private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
        private readonly EmployeeService _service;

        public EmployeeServiceTests()
        {
            MappingConfig.RegisterMappings();
            _repositoryMock = new Mock<IGenericRepository<Employee>>();
            _employeeRepositoryMock = new Mock<IEmployeeRepository>();
            _service = new EmployeeService(_repositoryMock.Object, _employeeRepositoryMock.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithNoFilter_ReturnsAllEmployees()
        {
            var employees = new List<Employee> { CreateEmployee("1", "John", "Doe"), CreateEmployee("2", "Jane", "Smith") };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);

            var result = await _service.GetAllAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.Count());
        }

        [Fact]
        public async Task GetAllAsync_WithIsActiveTrue_ReturnsActiveEmployees()
        {
            var activeEmployee = CreateEmployee("1", "John", "Doe");
            activeEmployee.Status = true;
            var inactiveEmployee = CreateEmployee("2", "Jane", "Smith");
            inactiveEmployee.Status = false;

            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(new List<Employee> { activeEmployee });

            var result = await _service.GetAllAsync(true);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value!);
        }

        [Fact]
        public async Task GetAllAsync_WithIsActiveFalse_ReturnsInactiveEmployees()
        {
            var inactiveEmployee = CreateEmployee("1", "Jane", "Smith");
            inactiveEmployee.Status = false;

            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(new List<Employee> { inactiveEmployee });

            var result = await _service.GetAllAsync(false);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value!);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsEmployeeDto()
        {
            var id = "1";
            var employee = CreateEmployee(id, "John", "Doe");

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(employee);

            var result = await _service.GetByIdAsync(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(id, result.Value!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ReturnsFailure()
        {
            var id = "99";

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

            var result = await _service.GetByIdAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Error);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithUniqueCedula_CreatesEmployeeWithIsActiveTrue()
        {
            var insertDto = new EmployeeInsertDto
            {
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                Cedula = "1234567890",
                PhoneNumber = "555-1234",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(new List<Employee>());
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);

            var result = await _service.AddAsync(insertDto);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(result.Value!.IsActive);
            _repositoryMock.Verify(r => r.AddAsync(It.Is<Employee>(e => e.Status == true)), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WithDuplicateCedula_ReturnsFailure()
        {
            var insertDto = new EmployeeInsertDto
            {
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                Cedula = "1234567890",
                PhoneNumber = "555-1234",
                DateOfBirth = new DateTime(1990, 1, 1)
            };
            var existingEmployee = CreateEmployee("existing-id", "Jane", "Smith");
            existingEmployee.IdCard = insertDto.Cedula;

            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(new List<Employee> { existingEmployee });

            var result = await _service.AddAsync(insertDto);

            Assert.False(result.IsSuccess);
            Assert.Contains("La cédula ya está registrada", result.Error);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenFound_UpdatesSuccessfully()
        {
            var id = "1";
            var existingEmployee = CreateEmployee(id, "John", "Doe");
            var updateDto = new EmployeeUpdateDto
            {
                FirstName = "John",
                LastName = "Updated",
                Address = "456 New St",
                Cedula = "1234567890",
                PhoneNumber = "555-9999",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingEmployee);
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(new List<Employee>());
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>())).ReturnsAsync(existingEmployee);

            var result = await _service.UpdateAsync(id, updateDto);

            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsFailure()
        {
            var id = "99";
            var updateDto = new EmployeeUpdateDto
            {
                FirstName = "John",
                LastName = "Updated",
                Address = "456 New St",
                Cedula = "1234567890",
                PhoneNumber = "555-9999",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

            var result = await _service.UpdateAsync(id, updateDto);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Error);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateCedula_ReturnsFailure()
        {
            var id = "1";
            var existingEmployee = CreateEmployee(id, "John", "Doe");
            existingEmployee.IdCard = "old-cedula";
            var updateDto = new EmployeeUpdateDto
            {
                FirstName = "John",
                LastName = "Updated",
                Address = "456 New St",
                Cedula = "duplicate-cedula",
                PhoneNumber = "555-9999",
                DateOfBirth = new DateTime(1990, 1, 1)
            };
            var conflictingEmployee = CreateEmployee("other-id", "Jane", "Smith");
            conflictingEmployee.IdCard = "duplicate-cedula";

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingEmployee);
            _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(new List<Employee> { conflictingEmployee });

            var result = await _service.UpdateAsync(id, updateDto);

            Assert.False(result.IsSuccess);
            Assert.Contains("La cédula ya está registrada", result.Error);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenFound_RemovesSuccessfully()
        {
            var id = "1";
            var employee = CreateEmployee(id, "John", "Doe");

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(employee);
            _repositoryMock.Setup(r => r.RemoveAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync(id);

            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.RemoveAsync(It.IsAny<Employee>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFailure()
        {
            var id = "99";

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

            var result = await _service.DeleteAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Error);
        }

        #endregion

        #region Paginate Tests

        [Fact]
        public async Task Paginate_ReturnsPaginatedResponse()
        {
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var employees = new List<Employee> { CreateEmployee("1", "John", "Doe"), CreateEmployee("2", "Jane", "Smith") };
            var paginatedResponse = new PaginatedResponse<Employee>
            {
                Items = employees,
                Pagination = pagination
            };

            _repositoryMock.Setup(r => r.Paginate(pagination)).ReturnsAsync(paginatedResponse);

            var result = await _service.Paginate(pagination);

            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
        }

        [Fact]
        public async Task Paginate_WithIsActiveFilter_ReturnsFilteredPaginatedResponse()
        {
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var activeEmployees = new List<Employee> { CreateEmployee("1", "John", "Doe") };
            activeEmployees[0].Status = true;
            var paginatedResponse = new PaginatedResponse<Employee>
            {
                Items = activeEmployees,
                Pagination = pagination
            };

            _repositoryMock.Setup(r => r.PaginateWhere(pagination, It.IsAny<Expression<Func<Employee, bool>>>())).ReturnsAsync(paginatedResponse);

            var result = await _service.Paginate(pagination, true);

            Assert.NotNull(result);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task Paginate_WithNoFilter_ReturnsAllPaginatedResponse()
        {
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var employees = new List<Employee> { CreateEmployee("1", "John", "Doe"), CreateEmployee("2", "Jane", "Smith") };
            var paginatedResponse = new PaginatedResponse<Employee>
            {
                Items = employees,
                Pagination = pagination
            };

            _repositoryMock.Setup(r => r.Paginate(pagination)).ReturnsAsync(paginatedResponse);

            var result = await _service.Paginate(pagination, null);

            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
        }

        #endregion

        #region GetHistoryAsync Tests

        [Fact]
        public async Task GetHistoryAsync_WhenEmployeeExists_ReturnsHistoryDtos()
        {
            var employeeId = "emp-1";
            var employee = CreateEmployee(employeeId, "John", "Doe");
            var history = new List<EmployeePosition>
            {
                CreateEmployeePosition(1, employeeId, 1, "Developer", "Engineering", DateTime.UtcNow.AddYears(-2), null),
                CreateEmployeePosition(2, employeeId, 2, "Senior Developer", "Engineering", DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddMonths(-6))
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync(employee);
            _employeeRepositoryMock.Setup(r => r.GetEmployeeHistoryWithDetailsAsync(employeeId)).ReturnsAsync(history);

            var result = await _service.GetHistoryAsync(employeeId);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value!.Count());
        }

        [Fact]
        public async Task GetHistoryAsync_WhenEmployeeNotFound_ReturnsFailure()
        {
            var employeeId = "nonexistent";

            _repositoryMock.Setup(r => r.GetByIdAsync(employeeId)).ReturnsAsync((Employee?)null);

            var result = await _service.GetHistoryAsync(employeeId);

            Assert.False(result.IsSuccess);
            Assert.Contains("Entity not found.", result.Error);
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

        private static EmployeePosition CreateEmployeePosition(int id, string employeeId, int positionId, string positionName, string departmentName, DateTime startDate, DateTime? endDate)
        {
            var department = new Department { Id = 1, Name = departmentName };
            var position = new Position { Id = positionId, Name = positionName, DepartmentId = 1, Department = department, Salary = 50000m };
            return new EmployeePosition
            {
                Id = id,
                EmployeeId = employeeId,
                PositionId = positionId,
                Position = position,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        #endregion
    }
}