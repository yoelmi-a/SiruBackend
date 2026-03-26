using SIRU.Core.Domain.Common;

namespace SIRU.Core.Domain.Entities
{
    public class Vacant : BaseEntity<string>
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Profile { get; set; }
        public required DateTime PublicationDate { get; set; }
        public DateTime? HiringDate { get; set; }
        public required int Status { get; set; }
        public ICollection<VacancyCandidate>? Candidates { get; set; }
    }
}
