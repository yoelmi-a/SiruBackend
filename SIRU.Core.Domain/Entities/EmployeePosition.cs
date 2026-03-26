namespace SIRU.Core.Domain.Entities
{
    public class EmployeePosition
    {
        public required int PositionId { get; set; }
        public required string EmployeeId { get; set; }
        public required DateTime StartDate { get; set; }
        public Employee? Employee { get; set; }
        public Position? Position { get; set; }
    }
}
