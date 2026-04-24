namespace SIRU.Core.Application.Dtos.Candidates
{
    public class CandidateInsertDto
    {
        public required string Names { get; set; }
        public required string LastNames { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
