using Microsoft.AspNetCore.Identity;
using SIRU.Core.Domain.Common.Enums;
using SIRU.Infrastructure.Identity.Entities;

namespace SIRU.Infrastructure.Identity.Seeds;

public static class DefaultAdmin
{
    public static async Task SeedAsync(UserManager<AuthAccount> userManager)
    {
        var adminEmail = "alcalayoelmi.a@gmail.com";

        var defaultAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (defaultAdmin == null)
        {
            defaultAdmin = new()
            {
                UserName = adminEmail,
                Email = "alcalayoelmi.a@gmail.com",
                EmailConfirmed = true,
                IdCard = "xxx-xxxxxxx-x",
                Name = "Yoelmi",
                LastName = "Alcalá"
            };

            await userManager.CreateAsync(defaultAdmin, "123P@ssword");
            await userManager.AddToRoleAsync(defaultAdmin, nameof(Roles.Admin));
        }
        
    }
}