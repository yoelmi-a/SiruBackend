using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Employees
{
    public class EmployeeInsertDto
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido.")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "La dirección es requerida.")]
        public required string Address { get; set; }

        [Required(ErrorMessage = "La cédula es requerida.")]
        public required string Cedula { get; set; }

        [Required(ErrorMessage = "El número de teléfono es requerido.")]
        public required string PhoneNumber { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es requerida.")]
        public DateTime DateOfBirth { get; set; }

        public string? Email { get; set; }
    }
}