using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SIRU.Core.Domain.Interfaces;
using SIRU.Infraestructure.Ranking.Helpers;
using SIRU.Infrastructure.Persistence.Contexts;

namespace SIRU.Infraestructure.Ranking.Services;

public class RankingProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRankingQueue _rankingQueue;
    private readonly PdfTextExtractor _pdfTextExtractor;
    private readonly IRankingService _rankingService;
    private readonly ILogger<RankingProcessingService> _logger;
    private readonly SemaphoreSlim _semaphore = new(3);

    public RankingProcessingService(
        IServiceScopeFactory scopeFactory,
        IRankingQueue rankingQueue,
        PdfTextExtractor pdfTextExtractor,
        IRankingService rankingService,
        ILogger<RankingProcessingService> logger)
    {
        _scopeFactory = scopeFactory;
        _rankingQueue = rankingQueue;
        _pdfTextExtractor = pdfTextExtractor;
        _rankingService = rankingService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var id in _rankingQueue.ReadAllAsync(stoppingToken))
        {
            await _semaphore.WaitAsync(stoppingToken);

            _ = ProcessAsync(id, stoppingToken);
        }
    }

    private async Task ProcessAsync(string vacancyCandidateId, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var application = await context.VacancyCandidates
                .Include(vc => vc.Vacant)
                .FirstOrDefaultAsync(vc => vc.Id == vacancyCandidateId, cancellationToken);

            if (application is null)
            {
                _logger.LogWarning("VacancyCandidate {Id} not found for ranking", vacancyCandidateId);
                return;
            }

            var cvText = await _pdfTextExtractor.ExtractTextAsync(application.CvUrl);
            if (string.IsNullOrWhiteSpace(cvText))
            {
                _logger.LogWarning("Could not extract text from CV for {Id}", vacancyCandidateId);
                application.Score = 0.0f;
            }
            else
            {
                var vacancyText = application.Vacant is not null
                    ? $"{application.Vacant.Profile} {application.Vacant.Description}"
                    : string.Empty;

                application.Score = await _rankingService.ComputeScoreAsync(cvText, vacancyText);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ranking for {Id}", vacancyCandidateId);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}