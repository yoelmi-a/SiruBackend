namespace SIRU.Core.Application.Dtos.Employees
{
    public class EmployeeListDto
    {
        public required string Id { get; set; }
        public required string FullName { get; set; }
        public required string Cedula { get; set; }
        public required string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }
}