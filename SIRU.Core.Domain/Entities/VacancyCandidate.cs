using SIRU.Core.Domain.Common;
using SIRU.Core.Domain.Common.Enums;

namespace SIRU.Core.Domain.Entities;

public class VacancyCandidate : BaseEntity<string>
{
    public required string VacantId { get; set; }
    public required string CandidateId { get; set; }
    public float Score { get; set; }
    public required CandidateStatus Status { get; set; }
    public required string CvUrl { get; set; }

    public Vacant? Vacant { get; set; }
    public Candidate? Candidate { get; set; }
}