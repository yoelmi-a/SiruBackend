using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Evaluations;

/// <summary>
/// DTO for creating a new evaluation criterion.
/// </summary>
public class CriterionInsertDto
{
    [Required(ErrorMessage = "El nombre del criterio es requerido.")]
    [StringLength(150, ErrorMessage = "El nombre del criterio no puede exceder 150 caracteres.")]
    public required string Name { get; set; }
}