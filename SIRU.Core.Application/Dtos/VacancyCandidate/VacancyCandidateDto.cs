using SIRU.Core.Application.Dtos.Candidate;
using SIRU.Core.Application.Dtos.Vacant;

namespace SIRU.Core.Application.Dtos.VacancyCandidate
{
    public class VacancyCandidateDto
    {
        public required string VacantId { get; set; }
        public required string CandidateId { get; set; }
        public required float Score { get; set; }
        public required int Status { get; set; }
        public VacantDto? Vacant { get; set; }
        public CandidateDto? Candidate { get; set; }
    }
}
