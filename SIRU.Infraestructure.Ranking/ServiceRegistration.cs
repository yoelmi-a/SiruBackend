using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infraestructure.Ranking.Helpers;
using SIRU.Infraestructure.Ranking.Queues;
using SIRU.Infraestructure.Ranking.Services;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infraestructure.Ranking;

public static class ServiceRegistration
{
    public static void AddRankingLayer(this IServiceCollection services)
    {
        services.AddSingleton<MLContext>();
        services.AddSingleton<PdfTextExtractor>();
        services.AddScoped<IRankingService, RankingService>();
        services.AddSingleton<IRankingQueue, RankingQueue>();
        services.AddHostedService<RankingProcessingService>();
    }
}