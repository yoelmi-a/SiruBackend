using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Application.Dtos.Candidate
{
    public class SaveCandidateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Names { get; set; } = string.Empty;
        public string LastNames { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public required string CvUrl { get; set; }
    }
}
