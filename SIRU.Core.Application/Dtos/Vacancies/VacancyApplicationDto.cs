using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Vacancies;

public class VacancyApplicationDto
{
    [Required]
    public string CandidateNames { get; set; }

    [Required]
    public string CandidateLastNames { get; set; }

    [Required]
    [EmailAddress]
    public string CandidateEmail { get; set; }

    [Required]
    public string CandidatePhoneNumber { get; set; }

    [Required]
    public IFormFile CvFile { get; set; }
}