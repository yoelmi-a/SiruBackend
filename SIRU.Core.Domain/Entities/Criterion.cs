using SIRU.Core.Domain.Common;

namespace SIRU.Core.Domain.Entities
{
    public class Criterion : BaseEntity<int>
    {
        public required string Name { get; set; }

        public required int PositionId { get; set; }
        public Position? Position { get; set; }

        public ICollection<EvaluationCriterion>? Evaluations { get; set; }
    }
}
