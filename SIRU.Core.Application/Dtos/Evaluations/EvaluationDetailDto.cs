namespace SIRU.Core.Application.Dtos.Evaluations;

public class EvaluationDetailDto
{
    public int CriterionId { get; set; }
    public required string CriterionName { get; set; }
    public required float Score { get; set; }
    public string? Observation { get; set; }
}