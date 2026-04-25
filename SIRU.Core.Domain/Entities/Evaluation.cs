using SIRU.Core.Domain.Common;

namespace SIRU.Core.Domain.Entities;

public class Evaluation : BaseEntity<string>
{
    public required int EmployeePositionId { get; set; }
    public required DateTime Date { get; set; }
    public float AverageScore { get; set; }

    public EmployeePosition? EmployeePosition { get; set; }
    public ICollection<EvaluationCriterion>? Criteria { get; set; }
}
