using Moq;
using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Application.Interfaces.Reports;
using SIRU.Core.Application.Services.Reports;
using SIRU.Core.Domain.Common.Pagination;

namespace SIRU.Tests.UnitTests.Services.Reports
{
    public class ReportServiceTests
    {
        private readonly Mock<IReportRepository> _repositoryMock;
        private readonly Mock<IPdfReportService> _pdfReportServiceMock;
        private readonly ReportService _service;

        public ReportServiceTests()
        {
            _repositoryMock = new Mock<IReportRepository>();
            _pdfReportServiceMock = new Mock<IPdfReportService>();
            _service = new ReportService(_repositoryMock.Object, _pdfReportServiceMock.Object);
        }

        #region HU-14 — GetAverageHiringTimeAsync

        [Fact]
        public async Task GetAverageHiringTimeAsync_WithNoClosedVacancies_ReturnsZeros()
        {
            _repositoryMock.Setup(r => r.GetAverageHiringTimeAsync())
                .ReturnsAsync(new HiringTimeReportDto { AverageDays = 0, TotalClosedVacancies = 0 });

            var result = await _service.GetAverageHiringTimeAsync();

            Assert.Equal(0, result.AverageDays);
            Assert.Equal(0, result.TotalClosedVacancies);
        }

        [Fact]
        public async Task GetAverageHiringTimeAsync_WithMultipleClosedVacancies_ReturnsCorrectAverage()
        {
            _repositoryMock.Setup(r => r.GetAverageHiringTimeAsync())
                .ReturnsAsync(new HiringTimeReportDto { AverageDays = 20, TotalClosedVacancies = 3 });

            var result = await _service.GetAverageHiringTimeAsync();

            Assert.Equal(20, result.AverageDays);
            Assert.Equal(3, result.TotalClosedVacancies);
        }

        [Fact]
        public async Task GetAverageHiringTimeAsync_WithSingleClosedVacancy_ReturnsExactDays()
        {
            _repositoryMock.Setup(r => r.GetAverageHiringTimeAsync())
                .ReturnsAsync(new HiringTimeReportDto { AverageDays = 5, TotalClosedVacancies = 1 });

            var result = await _service.GetAverageHiringTimeAsync();

            Assert.Equal(5, result.AverageDays);
            Assert.Equal(1, result.TotalClosedVacancies);
        }

        [Fact]
        public async Task GetAverageHiringTimeAsync_WithRoundingNeeded_ReturnsRoundedToTwoDecimals()
        {
            _repositoryMock.Setup(r => r.GetAverageHiringTimeAsync())
                .ReturnsAsync(new HiringTimeReportDto { AverageDays = 16.67f, TotalClosedVacancies = 3 });

            var result = await _service.GetAverageHiringTimeAsync();

            Assert.Equal(16.67f, result.AverageDays);
        }

        #endregion

        #region HU-15 — GetPerformanceByDepartmentAsync

        [Fact]
        public async Task GetPerformanceByDepartmentAsync_WithNoEvaluations_ReturnsEmptyList()
        {
            _repositoryMock.Setup(r => r.GetPerformanceByDepartmentAsync())
                .ReturnsAsync(new List<DepartmentPerformanceDto>());

            var result = await _service.GetPerformanceByDepartmentAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPerformanceByDepartmentAsync_ResultsSortedByScoreDescending()
        {
            var departments = new List<DepartmentPerformanceDto>
            {
                new() { DepartmentName = "IT", AverageScore = 3.5f, EmployeeCount = 2 },
                new() { DepartmentName = "HR", AverageScore = 4.2f, EmployeeCount = 3 },
                new() { DepartmentName = "Dev", AverageScore = 3.8f, EmployeeCount = 1 }
            };
            _repositoryMock.Setup(r => r.GetPerformanceByDepartmentAsync())
                .ReturnsAsync(departments);

            var result = (await _service.GetPerformanceByDepartmentAsync()).ToList();

            Assert.Equal("IT", result[0].DepartmentName);
            Assert.Equal("HR", result[1].DepartmentName);
            Assert.Equal("Dev", result[2].DepartmentName);
        }

        [Fact]
        public async Task GetPerformanceByDepartmentAsync_ScoresRoundedToTwoDecimals()
        {
            var departments = new List<DepartmentPerformanceDto>
            {
                new() { DepartmentName = "HR", AverageScore = 4.33f, EmployeeCount = 2 }
            };
            _repositoryMock.Setup(r => r.GetPerformanceByDepartmentAsync())
                .ReturnsAsync(departments);

            var result = (await _service.GetPerformanceByDepartmentAsync()).ToList();

            Assert.Equal(4.33f, result[0].AverageScore);
        }

        [Fact]
        public async Task GetPerformanceByDepartmentAsync_EmployeeCountMatchesEvaluationCount()
        {
            var departments = new List<DepartmentPerformanceDto>
            {
                new() { DepartmentName = "HR", AverageScore = 4.0f, EmployeeCount = 3 }
            };
            _repositoryMock.Setup(r => r.GetPerformanceByDepartmentAsync())
                .ReturnsAsync(departments);

            var result = (await _service.GetPerformanceByDepartmentAsync()).ToList();

            Assert.Equal(3, result[0].EmployeeCount);
        }

        #endregion

        #region HU-16 — GetEmployeeReportAsync

        [Fact]
        public async Task GetEmployeeReportAsync_WithPagination_ReturnsCorrectPage()
        {
            var pagination = new Pagination { PageNumber = 2, PageSize = 2 };
            var response = new PaginatedResponse<EmployeeReportDto>
            {
                Items = new List<EmployeeReportDto>
                {
                    new() { Id = "emp3", FullName = "Third User", Cedula = "C", Position = "Dev", Department = "IT", IsActive = true },
                    new() { Id = "emp4", FullName = "Fourth User", Cedula = "D", Position = "QA", Department = "IT", IsActive = false }
                },
                Pagination = new Pagination { PageNumber = 2, PageSize = 2, TotalCount = 5 }
            };
            _repositoryMock.Setup(r => r.GetEmployeeReportAsync(pagination, null))
                .ReturnsAsync(response);

            var result = await _service.GetEmployeeReportAsync(pagination, null);

            Assert.Equal(2, result.Items.Count());
            Assert.Equal(5, result.Pagination.TotalCount);
            Assert.Equal(2, result.Pagination.PageNumber);
            Assert.Equal(2, result.Pagination.PageSize);
        }

        [Fact]
        public async Task GetEmployeeReportAsync_WithActiveFilter_PassesFilterToRepository()
        {
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var response = new PaginatedResponse<EmployeeReportDto>
            {
                Items = new List<EmployeeReportDto>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalCount = 0 }
            };
            _repositoryMock.Setup(r => r.GetEmployeeReportAsync(pagination, true))
                .ReturnsAsync(response);

            await _service.GetEmployeeReportAsync(pagination, true);

            _repositoryMock.Verify(r => r.GetEmployeeReportAsync(pagination, true), Times.Once);
        }

        [Fact]
        public async Task GetEmployeeReportAsync_WithInactiveFilter_PassesFilterToRepository()
        {
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var response = new PaginatedResponse<EmployeeReportDto>
            {
                Items = new List<EmployeeReportDto>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalCount = 0 }
            };
            _repositoryMock.Setup(r => r.GetEmployeeReportAsync(pagination, false))
                .ReturnsAsync(response);

            await _service.GetEmployeeReportAsync(pagination, false);

            _repositoryMock.Verify(r => r.GetEmployeeReportAsync(pagination, false), Times.Once);
        }

        [Fact]
        public async Task GetEmployeeReportAsync_WithUnassignedEmployee_ReturnsUnassigned()
        {
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var response = new PaginatedResponse<EmployeeReportDto>
            {
                Items = new List<EmployeeReportDto>
                {
                    new() { Id = "emp1", FullName = "John Doe", Cedula = "ABC123", Position = "Unassigned", Department = "Unassigned", IsActive = true }
                },
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalCount = 1 }
            };
            _repositoryMock.Setup(r => r.GetEmployeeReportAsync(pagination, null))
                .ReturnsAsync(response);

            var result = await _service.GetEmployeeReportAsync(pagination, null);

            var item = result.Items.Single();
            Assert.Equal("Unassigned", item.Position);
            Assert.Equal("Unassigned", item.Department);
        }

        [Fact]
        public async Task GetEmployeeReportAsync_WithNoEmployees_ReturnsEmptyItemsWithZeroTotal()
        {
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var response = new PaginatedResponse<EmployeeReportDto>
            {
                Items = new List<EmployeeReportDto>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalCount = 0 }
            };
            _repositoryMock.Setup(r => r.GetEmployeeReportAsync(pagination, null))
                .ReturnsAsync(response);

            var result = await _service.GetEmployeeReportAsync(pagination, null);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.Pagination.TotalCount);
        }

        #endregion

        #region HU-17 — Export Reports

        [Fact]
        public async Task ExportHiringTimeAsync_ReturnsPdfBytes()
        {
            var dto = new HiringTimeReportDto { AverageDays = 15.5f, TotalClosedVacancies = 4 };
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _repositoryMock.Setup(r => r.GetAverageHiringTimeAsync()).ReturnsAsync(dto);
            _pdfReportServiceMock.Setup(p => p.GenerateHiringTimeReport(dto)).Returns(pdfBytes);

            var result = await _service.ExportHiringTimeAsync();

            Assert.Equal(pdfBytes, result);
        }

        [Fact]
        public async Task ExportHiringTimeAsync_WithZeroValues_ReturnsPdfBytes()
        {
            var dto = new HiringTimeReportDto { AverageDays = 0, TotalClosedVacancies = 0 };
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _repositoryMock.Setup(r => r.GetAverageHiringTimeAsync()).ReturnsAsync(dto);
            _pdfReportServiceMock.Setup(p => p.GenerateHiringTimeReport(dto)).Returns(pdfBytes);

            var result = await _service.ExportHiringTimeAsync();

            Assert.Equal(pdfBytes, result);
        }

        [Fact]
        public async Task ExportPerformanceByDepartmentAsync_ReturnsPdfBytes()
        {
            var data = new List<DepartmentPerformanceDto>
            {
                new() { DepartmentName = "IT", AverageScore = 4.2f, EmployeeCount = 3 },
                new() { DepartmentName = "HR", AverageScore = 3.8f, EmployeeCount = 2 }
            };
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _repositoryMock.Setup(r => r.GetPerformanceByDepartmentAsync()).ReturnsAsync(data);
            _pdfReportServiceMock.Setup(p => p.GeneratePerformanceByDepartmentReport(data)).Returns(pdfBytes);

            var result = await _service.ExportPerformanceByDepartmentAsync();

            Assert.Equal(pdfBytes, result);
        }

        [Fact]
        public async Task ExportPerformanceByDepartmentAsync_WithEmptyList_ReturnsPdfBytes()
        {
            var data = new List<DepartmentPerformanceDto>();
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _repositoryMock.Setup(r => r.GetPerformanceByDepartmentAsync()).ReturnsAsync(data);
            _pdfReportServiceMock.Setup(p => p.GeneratePerformanceByDepartmentReport(data)).Returns(pdfBytes);

            var result = await _service.ExportPerformanceByDepartmentAsync();

            Assert.Equal(pdfBytes, result);
        }

        [Fact]
        public async Task ExportEmployeesAsync_ReturnsPdfBytes()
        {
            var data = new List<EmployeeReportDto>
            {
                new() { Id = "e1", FullName = "John Doe", Cedula = "ABC123", Position = "Developer", Department = "IT", IsActive = true },
                new() { Id = "e2", FullName = "Jane Smith", Cedula = "XYZ789", Position = "Designer", Department = "Design", IsActive = true }
            };
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _repositoryMock.Setup(r => r.GetAllEmployeesAsync()).ReturnsAsync(data);
            _pdfReportServiceMock.Setup(p => p.GenerateEmployeeReport(data)).Returns(pdfBytes);

            var result = await _service.ExportEmployeesAsync();

            Assert.Equal(pdfBytes, result);
        }

        [Fact]
        public async Task ExportEmployeesAsync_WithUnassignedPosition_ReturnsPdfBytes()
        {
            var data = new List<EmployeeReportDto>
            {
                new() { Id = "e1", FullName = "Bob Jones", Cedula = "AAA111", Position = "Unassigned", Department = "Unassigned", IsActive = false }
            };
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
            _repositoryMock.Setup(r => r.GetAllEmployeesAsync()).ReturnsAsync(data);
            _pdfReportServiceMock.Setup(p => p.GenerateEmployeeReport(data)).Returns(pdfBytes);

            var result = await _service.ExportEmployeesAsync();

            Assert.Equal(pdfBytes, result);
        }

        #endregion
    }
}