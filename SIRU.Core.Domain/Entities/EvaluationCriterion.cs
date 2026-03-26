namespace SIRU.Core.Domain.Entities
{
    public class EvaluationCriterion
    {
        public required string EvaluationId { get; set; }
        public required int CriteriaId { get; set; }
        public required float Score { get; set; }
        public string? Observation { get; set; }

        public Evaluation? Evaluation { get; set; }
        public Criterion? Criterion { get; set; }
    }
}
