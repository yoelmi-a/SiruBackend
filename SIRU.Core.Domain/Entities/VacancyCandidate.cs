namespace SIRU.Core.Domain.Entities
{
    public class VacancyCandidate
    {
        public required string VacantId { get; set; }
        public required string CandidateId { get; set; }
        public required float Score { get; set; }
        public required int Status { get; set; }

        public Vacant? Vacant { get; set; }
        public Candidate? Candidate { get; set; }
    }
}
