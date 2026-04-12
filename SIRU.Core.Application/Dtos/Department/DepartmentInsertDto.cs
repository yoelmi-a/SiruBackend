using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Department
{
    public class DepartmentInsertDto
    {
        [Required(ErrorMessage = "El nombre del departamento es requerido.")]
        [MinLength(1, ErrorMessage = "El nombre del departamento no puede estar vacío.")]
        public required string Name { get; set; }
    }
}
