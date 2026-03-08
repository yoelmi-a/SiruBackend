using SIRU.Core.Domain.Common;

namespace SIRU.Core.Domain.Entities
{
    public class Department : BaseEntity<int>
    {
        public required string Name { get; set; }

        public ICollection<Position>? Positions { get; set; }
    }
}
