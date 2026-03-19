using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Position
{
    public class PositionInsertDto
    {
        [Required(ErrorMessage = "El nombre de la posición es requerido.")]
        [MinLength(1, ErrorMessage = "El nombre de la posición no puede estar vacío.")]
        public required string Name { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser un valor positivo.")]
        public decimal Salary { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID del departamento debe ser válido.")]
        public int DepartmentId { get; set; }
    }
}
