using Microsoft.Extensions.DependencyInjection;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Application.Interfaces.Reports;
using SIRU.Infrastructure.Shared.Pdf;
using SIRU.Infrastructure.Shared.Storage;

namespace SIRU.Infrastructure.Shared;

public static class ServiceRegistration
{
    public static void AddSharedLayer(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IPdfReportService, PdfReportService>();
    }
}