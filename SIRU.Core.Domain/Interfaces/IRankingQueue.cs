namespace SIRU.Core.Domain.Interfaces;

public interface IRankingQueue
{
    ValueTask EnqueueAsync(string vacancyCandidateId);
    IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken);
}