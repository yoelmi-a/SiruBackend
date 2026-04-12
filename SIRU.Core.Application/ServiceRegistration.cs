using Microsoft.Extensions.DependencyInjection;
using SIRU.Core.Application.Interfaces.Users;
using SIRU.Core.Application.Services.Users;

namespace SIRU.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Services
            // Users
            services.AddScoped<IUserService, UserService>();
            #endregion
        }
    }
}
