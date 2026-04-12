using System.ComponentModel.DataAnnotations;

namespace SIRU.Core.Application.Dtos.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
