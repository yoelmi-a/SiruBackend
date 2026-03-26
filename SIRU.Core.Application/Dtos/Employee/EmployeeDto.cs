using SIRU.Core.Application.Dtos.EmployeePosition;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Application.Dtos.Employee
{
    public class EmployeeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Names { get; set; } = string.Empty;
        public string LastNames { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public DateTime Birthdate { get; set; }
        public bool Status { get; set; }
    }
}
