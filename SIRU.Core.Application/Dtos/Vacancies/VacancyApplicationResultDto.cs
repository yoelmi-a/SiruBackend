using SIRU.Core.Domain.Common.Enums;

namespace SIRU.Core.Application.Dtos.Vacancies;

public class VacancyApplicationResultDto
{
    public required string ApplicationId { get; set; }
    public required string VacantId { get; set; }
    public required string CandidateId { get; set; }
    public required string CandidateFullName { get; set; }
    public required string CvUrl { get; set; }
    public required CandidateStatus Status { get; set; }
    public required float Score { get; set; }
}