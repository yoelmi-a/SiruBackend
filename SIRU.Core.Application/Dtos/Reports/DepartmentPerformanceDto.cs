namespace SIRU.Core.Application.Dtos.Reports;

public class DepartmentPerformanceDto
{
    public required string DepartmentName { get; set; }
    public required float AverageScore { get; set; }
    public required int EmployeeCount { get; set; }
}