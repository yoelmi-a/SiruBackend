using SIRU.Core.Domain.Common;

namespace SIRU.Core.Domain.Entities
{
    public class Employee : Person
    {
        public required string Address { get; set; }
        public required string IdCard { get; set; }
        public required DateTime Birthdate { get; set; }
        public required bool Status { get; set; }

        public ICollection<EmployeePosition>? PositionsOccupied { get; set; }
        public ICollection<Evaluation>? Evaluations { get; set; }
    }
}
