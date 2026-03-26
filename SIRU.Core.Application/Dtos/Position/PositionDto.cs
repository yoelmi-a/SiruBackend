using SIRU.Core.Application.Dtos.Department;
using SIRU.Core.Application.Dtos.EmployeePosition;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Application.Dtos.Position
{
    public class PositionDto
    {
        public required int Id {  get; set; }
        public required string Name { get; set; }
        public required decimal Salary { get; set; }
        public DepartmentDto? Department { get; set; }
    }
}
