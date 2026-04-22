namespace SIRU.Core.Domain.Interfaces;

public interface IRankingService
{
    Task<float> ComputeScoreAsync(string cvText, string vacancyText);
}