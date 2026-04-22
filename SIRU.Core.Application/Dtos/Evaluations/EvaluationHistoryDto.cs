namespace SIRU.Core.Application.Dtos.Evaluations;

public class EvaluationHistoryDto
{
    public required string Id { get; set; }
    public required DateTime Date { get; set; }
    public required float AverageScore { get; set; }
    public required string PositionName { get; set; }
    public required List<EvaluationHistoryCriterionDto> Criteria { get; set; }
}

public class EvaluationHistoryCriterionDto
{
    public required string Name { get; set; }
    public required float Score { get; set; }
    public string? Observation { get; set; }
}