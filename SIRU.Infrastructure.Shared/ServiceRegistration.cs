using Microsoft.Extensions.DependencyInjection;

namespace SIRU.Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedLayer(this IServiceCollection services)
        {
            // Aquí se registrarán servicios compartidos como Email, SMS, etc.
        }
    }
}
