using Microsoft.AspNetCore.Identity;
using SIRU.Core.Domain.Common.Enums;

namespace SIRU.Infrastructure.Identity.Seeds;

public static class DefaultRoles
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!roleManager.Roles.Any())
        {
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Owner)));
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Admin)));
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.SuperAdmin)));
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Delivery)));
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Mechanic)));
            await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Clerk)));
        }
    }
}