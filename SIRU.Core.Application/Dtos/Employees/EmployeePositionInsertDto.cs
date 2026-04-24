using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Employees;

public class EmployeePositionInsertDto
{
    [Required(ErrorMessage = "El ID del empleado es requerido.")]
    public required string EmployeeId { get; set; }

    [Required(ErrorMessage = "El ID del cargo es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del cargo debe ser mayor a 0.")]
    public required int PositionId { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
}

public class EmployeePositionDto
{
    public required int Id { get; set; }
    public required string EmployeeId { get; set; }
    public required string EmployeeFullName { get; set; }
    public required int PositionId { get; set; }
    public required string PositionName { get; set; }
    public required string DepartmentName { get; set; }
    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}