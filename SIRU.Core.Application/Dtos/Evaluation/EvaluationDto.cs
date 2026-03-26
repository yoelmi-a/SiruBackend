using SIRU.Core.Application.Dtos.Employee;
using SIRU.Core.Application.Dtos.EvaluationCriterion;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIRU.Core.Application.Dtos.Evaluation
{
    public class EvaluationDto
    {
        public required int Id {  get; set; }
        public required DateTime Date { get; set; }
        public EmployeeDto? Employee { get; set; }
        public ICollection<EvaluationCriterionDto>? Criteria { get; set; }

    }
}
