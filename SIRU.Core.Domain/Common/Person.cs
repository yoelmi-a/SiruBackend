namespace SIRU.Core.Domain.Common
{
    public class Person : BaseEntity<string>
    {
        public required string Names { get; set; }
        public required string LastNames { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
