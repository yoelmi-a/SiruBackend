using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Vacants
{
    public class UpdateVacantDto
    {
        [Required(ErrorMessage = "El título es requerido.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "La descripción es requerida.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "El perfil es requerido.")]
        public required string Profile { get; set; }

        [Required(ErrorMessage = "El estado es requerido.")]
        [RegularExpression("Open|Closed|Cancelled", ErrorMessage = "El estado debe ser Open, Closed o Cancelled.")]
        public required string Status { get; set; }
    }
}