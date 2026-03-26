using SIRU.Core.Domain.Common;

namespace SIRU.Core.Domain.Entities
{
    public class Position : BaseEntity<int>
    {
        public required string Name { get; set; }
        public required decimal Salary { get; set; }
        public required int DepartmentId { get; set; }
        public Department? Department { get; set; }
        public ICollection<EmployeePosition>? Employees { get; set; }
    }
}
