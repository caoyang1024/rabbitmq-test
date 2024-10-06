namespace PriceEngine.Tools.StressTest;

public class StressTestPublisher(
    ILogger<StressTestPublisher> logger,
    StressTestPublisherConfig config,
    IMessagePublisher publisher) : BackgroundService
{
    private CancellationToken _cancellationToken;

    private long _count = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Task> jobs = [];

        await SendStartMessage();

        for (int i = 0; i < config.NumberOfThreads; i++)
        {
            jobs.Add(StartWorker(stoppingToken));
        }

        await Task.WhenAll(jobs);

        logger.LogInformation($"total {_count} sent");

        await SendStopMessage();
    }

    private async Task StartWorker(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;

        DateTimeOffset startTime = DateTimeOffset.Now;
        DateTimeOffset endTime = startTime.AddSeconds(config.DurationInSeconds);

        TimeSpan interval = TimeSpan.FromMilliseconds(1000.0 / config.MessagesPerSecondPerThread);

        logger.LogInformation($"delay interval: {interval.TotalMilliseconds:F1} ms");

        Timer timer = new Timer(Callback, null, interval, interval);

        var start = DateTimeOffset.UtcNow;

        await Task.Delay(endTime - DateTimeOffset.UtcNow, cancellationToken);

        var end = DateTimeOffset.UtcNow;

        logger.LogInformation($"ran {(end - start).TotalSeconds:F1} seconds");

        timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    private async Task SendStartMessage()
    {
        var startMessage = new SessionMessage
        {
            EventSentTimeEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            EventReceiveTimeEpoch = 0,
            Status = "START"
        };

        await publisher.PublishAsync(startMessage, _cancellationToken);
    }

    private async Task SendStopMessage()
    {
        var stopMessage = new SessionMessage
        {
            EventSentTimeEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            EventReceiveTimeEpoch = 0,
            Status = "STOP"
        };

        await publisher.PublishAsync(stopMessage, _cancellationToken);
    }

    private PriceObject GetPrice(string symbol)
    {
        return new PriceObject
        {
            MarketId = Random.Shared.Next(1, 1000),
            Symbol = symbol,
            EventSentTimeEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CorrelationId = Guid.NewGuid().ToString("N"),
            LPOutTime = DateTimeOffset.UtcNow,
            SourceInTime = DateTimeOffset.UtcNow,
            ServiceInTime = DateTimeOffset.UtcNow,
            ServiceOutTime = DateTimeOffset.UtcNow,
            FeederSource = "Test",
            Bid = 69882.0m,
            BidVolume = 1.0m,
            Ask = 69882.0m,
            AskVolume = 1.1m,
            Mid = 69882.0m,
            DepthMarketId = 12,
            Model = nameof(StressTestPublisher),
            EventReceiveTimeEpoch = 0,
        };
    }

    private async void Callback(object state)
    {
        var message = GetPrice("BTCUSD");

        await publisher.PublishAsync(message, _cancellationToken);

        Interlocked.Increment(ref _count);
    }
}