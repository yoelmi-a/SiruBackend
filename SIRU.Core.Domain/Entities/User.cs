using SIRU.Core.Domain.Common;

namespace SIRU.Core.Domain.Entities
{
    public class User : BaseEntity<string>
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
