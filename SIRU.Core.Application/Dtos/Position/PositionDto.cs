namespace SIRU.Core.Application.Dtos.Position
{
    public class PositionDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Salary { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}
