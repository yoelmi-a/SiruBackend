using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Evaluations;

public class EvaluationInsertDto
{
    [Required(ErrorMessage = "El ID del empleado es requerido.")]
    public required string EmployeeId { get; set; }

    [Required(ErrorMessage = "La fecha de evaluación es requerida.")]
    public DateTime EvaluationDate { get; set; }

    [MinLength(1, ErrorMessage = "Debe incluir al menos un criterio.")]
    public required List<EvaluationCriterionInsertDto> Criteria { get; set; }
}