using Microsoft.AspNetCore.Identity;
using SIRU.Core.Application.Dtos.Users;
using SIRU.Core.Application.Interfaces.Users;
using SIRU.Core.Domain.Common.Enums;

namespace SIRU.Infrastructure.Identity.Seeds;

public static class DefaultSuperAdmin
{
    public static async Task SeedAsync(UserManager<IdentityUser> userManager, IUserService userService)
    {
        IdentityUser user = new()
        {
            UserName = "alcalayoelmi.a@gmail.com",
            Email = "alcalayoelmi.a@gmail.com",
            EmailConfirmed = true,
        };
        
        var superAdmin = await userManager.FindByEmailAsync(user.Email);

        if (superAdmin == null)
        {
            await userManager.CreateAsync(user, "123P@ssword");
            await userService.AddUserAsync(new SaveUserDto()
            {
                Id = user.Id,
                Name = "Yoelmi",
                LastName = "Alcalá",
                Email = user.Email
            });
            await userManager.AddToRoleAsync(user, nameof(Roles.SuperAdmin));
        }
        
    }
}