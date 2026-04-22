namespace SIRU.Core.Application.Dtos.Employees;

/// <summary>
/// DTO representing a single position assignment in an employee's work history.
/// </summary>
public class EmployeeHistoryDto
{
    public required string PositionName { get; set; }
    public required string DepartmentName { get; set; }
    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}