using SIRU.Core.Domain.Common;

namespace SIRU.Core.Domain.Entities
{
    public class Candidate : Person
    {
        public required string CvUrl { get; set; }

        public ICollection<VacancyCandidate>? VacanciesApplied { get; set; }
    }
}
