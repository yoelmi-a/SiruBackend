using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Departments
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
