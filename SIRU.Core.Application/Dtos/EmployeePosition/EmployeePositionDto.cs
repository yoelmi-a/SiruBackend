using SIRU.Core.Application.Dtos.Employee;
using SIRU.Core.Application.Dtos.Position;

namespace SIRU.Core.Application.Dtos.EmployeePosition
{
    public class EmployeePositionDto
    {
        public required int PositionId { get; set; }
        public required string EmployeeId { get; set; }
        public required DateTime StartDate { get; set; }
        public EmployeeDto? Employee { get; set; }
        public PositionDto? Position { get; set; }
    }
}
