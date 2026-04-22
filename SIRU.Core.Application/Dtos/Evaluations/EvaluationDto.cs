namespace SIRU.Core.Application.Dtos.Evaluations;

public class EvaluationDto
{
    public required string Id { get; set; }
    public required string EmployeeId { get; set; }
    public required string EmployeeFullName { get; set; }
    public required string PositionName { get; set; }
    public required DateTime Date { get; set; }
    public required float AverageScore { get; set; }
    public required List<EvaluationDetailDto> Criteria { get; set; }
}