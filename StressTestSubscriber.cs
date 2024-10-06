using System.Threading.Channels;

namespace PriceEngine.Tools.StressTest;

public sealed class StressTestSubscriber(ILogger<StressTestSubscriber> logger, IMessageSubscriber subscriber) : BackgroundService
{
    private readonly Channel<(PriceObject Price, long Latency)> _prices =
        Channel.CreateUnbounded<(PriceObject Price, long Latency)>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    private readonly List<(PriceObject Price, long Latency)> _latencies = new(1_000_000);

    private DateTimeOffset _start;
    private DateTimeOffset _stop;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            subscriber.MessageReceived += OnMessageReceived;

            await subscriber.SubscribeAsync($"{nameof(PriceObject)}.*", 3, stoppingToken);
            await subscriber.SubscribeAsync(SessionMessage.SessionTopic, cancellationToken: stoppingToken);

            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var px in _prices.Reader.ReadAllAsync(stoppingToken))
                    {
                        _latencies.Add(px);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }, stoppingToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private void OnMessageReceived(object sender, IEventMessage message)
    {
        if (message is PriceObject price)
        {
            price.EventReceiveTimeEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _prices.Writer.TryWrite((price, price.EventReceiveTimeEpoch - price.EventSentTimeEpoch));
        }
        else if (message is SessionMessage session)
        {
            if (session.Status == "START")
            {
                logger.LogTrace("start session: {@session}", session);

                _start = DateTimeOffset.UtcNow;
            }
            else if (session.Status == "STOP")
            {
                logger.LogTrace("stop session: {@session}", session);

                _stop = DateTimeOffset.UtcNow;

                Calculate();

                _latencies.Clear();
            }
        }
    }

    private void Calculate()
    {
        logger.LogInformation($"Total : {_latencies.Count}");
        logger.LogInformation($"Min : {_latencies.MinBy(x => x.Latency).Latency:F1}");
        logger.LogInformation($"Max : {_latencies.MaxBy(x => x.Latency).Latency:F1}");
        logger.LogInformation($"Ave : {_latencies.Average(x => x.Latency):F1}");
        logger.LogInformation($"Median : {_latencies.OrderByDescending(x => x.Latency).Skip((int)(_latencies.Count * 0.5)).First().Latency:F1}");
        logger.LogInformation($"99p : {_latencies.OrderByDescending(x => x.Latency).Skip((int)(_latencies.Count * 0.01)).First().Latency:F1}");

        var earliest = _latencies.MinBy(x => x.Price.EventReceiveTimeEpoch).Price.EventReceiveTimeEpoch;
        var latest = _latencies.MaxBy(x => x.Price.EventReceiveTimeEpoch).Price.EventReceiveTimeEpoch;

        logger.LogInformation($"Earliest : {earliest}");
        logger.LogInformation($"Latest : {latest}");
        logger.LogInformation($"{TimeSpan.FromMilliseconds(latest - earliest).TotalSeconds:F1} seconds");
        logger.LogInformation($"RPS: {_latencies.Count * decimal.One / ((latest - earliest) * decimal.One / 1000.0m):F1}");
    }
}