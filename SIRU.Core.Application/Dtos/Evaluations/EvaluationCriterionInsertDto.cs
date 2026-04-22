using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Evaluations;

public class EvaluationCriterionInsertDto
{
    [Required(ErrorMessage = "El ID del criterio es requerido.")]
    public required int CriterionId { get; set; }

    [Required(ErrorMessage = "La puntuación es requerida.")]
    [Range(0.0, 5.0, ErrorMessage = "La puntuación debe estar entre 0.0 y 5.0.")]
    public required float Score { get; set; }

    [StringLength(500, ErrorMessage = "La observación no puede exceder 500 caracteres.")]
    public string? Observation { get; set; }
}