namespace SIRU.Core.Application.Dtos.Employees
{
    public class EmployeeDto
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Address { get; set; }
        public required string Cedula { get; set; }
        public required string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }
}