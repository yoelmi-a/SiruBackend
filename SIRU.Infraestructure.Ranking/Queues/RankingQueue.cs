using System.Threading.Channels;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Infraestructure.Ranking.Queues;

public class RankingQueue : IRankingQueue
{
    private readonly Channel<string> _channel;

    public RankingQueue()
    {
        _channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
    }

    public async ValueTask EnqueueAsync(string vacancyCandidateId)
    {
        await _channel.Writer.WriteAsync(vacancyCandidateId);
    }

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}