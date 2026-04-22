using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Vacants
{
    public class SaveVacantDto
    {
        [Required(ErrorMessage = "El título es requerido.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "La descripción es requerida.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "El perfil es requerido.")]
        public required string Profile { get; set; }

        [Required(ErrorMessage = "El ID del puesto es requerido.")]
        public required int PositionId { get; set; }
    }
}