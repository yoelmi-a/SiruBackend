using SIRU.Core.Application.Dtos.Criterion;
using SIRU.Core.Application.Dtos.Evaluation;

namespace SIRU.Core.Application.Dtos.EvaluationCriterion
{
    public class EvaluationCriterionDto
    {
        public required string EvaluationId { get; set; }
        public required int CriteriaId { get; set; }
        public required float Score { get; set; }
        public string? Observation { get; set; }
        public EvaluationDto? Evaluation { get; set; }
        public CriterionDto? Criterion { get; set; }
    }
}
