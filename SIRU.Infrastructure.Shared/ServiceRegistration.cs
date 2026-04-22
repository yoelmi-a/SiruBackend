using Microsoft.Extensions.DependencyInjection;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Infrastructure.Shared.Storage;

namespace SIRU.Infrastructure.Shared;

public static class ServiceRegistration
{
    public static void AddSharedLayer(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, FileStorageService>();
    }
}