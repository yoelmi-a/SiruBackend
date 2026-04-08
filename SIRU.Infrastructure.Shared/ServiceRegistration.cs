using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIRU.Core.Application.Interfaces.Shared;
using SIRU.Core.Domain.Settings;
using SIRU.Infrastructure.Shared.Services;

namespace SIRU.Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();
            #endregion
        }
    }
}
