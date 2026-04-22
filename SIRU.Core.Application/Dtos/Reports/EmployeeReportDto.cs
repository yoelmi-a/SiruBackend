namespace SIRU.Core.Application.Dtos.Reports;

public class EmployeeReportDto
{
    public required string Id { get; set; }
    public required string FullName { get; set; }
    public required string Cedula { get; set; }
    public required string Position { get; set; }
    public required string Department { get; set; }
    public required bool IsActive { get; set; }
}