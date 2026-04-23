using Microsoft.AspNetCore.Identity;

namespace SIRU.Infrastructure.Identity.Entities
{
    public class AuthAccount : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string IdCard { get; set; }
    }
}
