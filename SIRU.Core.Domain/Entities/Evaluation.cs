using SIRU.Core.Domain.Common;

namespace SIRU.Core.Domain.Entities
{
    public class Evaluation : BaseEntity<string>
    {
        public required string EmployeeId { get; set; }
        public required DateTime Date { get; set; }

        public Employee? Employee { get; set; }
        public ICollection<EvaluationCriterion>? Criteria { get; set; }
    }
}
