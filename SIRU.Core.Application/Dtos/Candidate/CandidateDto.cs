namespace SIRU.Core.Application.Dtos.Candidate
{
    public class CandidateDto
    {
        public required string Id { get; set; }
        public required string Names { get; set; }
        public required string LastNames { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string CvUrl { get; set; }
    }
}
