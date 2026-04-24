namespace SIRU.Core.Application.Dtos.Candidates
{
    public class CandidateUpdateDto
    {
        public required string Id { get; set; }
        public required string Names { get; set; }
        public required string LastNames { get; set; }
        public string? Email { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
