using System.Collections.Concurrent;
using System.Threading.Channels;
using MessagePack;
using NATS.Client.Core;

namespace PriceEngine.Tools.StressTest.Nats;

public sealed class NatsSubscriber : IMessageSubscriber
{
    private readonly ILogger<NatsSubscriber> _logger;
    private readonly NatsConnection _connection;

    private readonly ConcurrentDictionary<string, string> _subscriptions = new();
    private readonly ConcurrentDictionary<string, Type> _types = new();

    public event EventHandler<IEventMessage> MessageReceived;

    public NatsSubscriber(ILogger<NatsSubscriber> logger, ILoggerFactory loggerFactory, NatsConfig config)
    {
        _logger = logger;
        _connection = new NatsConnection(NatsConfigProvider.GetConfig(config.Endpoint, $"{nameof(NatsSubscriber)}_{Guid.NewGuid():D}", loggerFactory));

        _connection.ConnectionOpened += ConnectionOnConnectionOpened;
        _connection.ConnectionDisconnected += ConnectionOnConnectionDisconnected;
        _connection.MessageDropped += ConnectionOnMessageDropped;
        _connection.ReconnectFailed += ConnectionOnReconnectFailed;
    }

    public async Task SubscribeAsync(string topic, int worker = 1, CancellationToken cancellationToken = default)
    {
        if (_subscriptions.TryAdd(topic, ""))
        {
            _logger.LogInformation("SUBSCRIBED TO TOPIC: {topic}", topic);

            var sub = await _connection.SubscribeCoreAsync<MessageWrapper>(topic,
                   opts: new NatsSubOpts
                   {
                       ChannelOpts = new NatsSubChannelOpts
                       {
                           Capacity = 10000,
                           FullMode = topic.StartsWith(nameof(PriceObject)) ? BoundedChannelFullMode.DropOldest : BoundedChannelFullMode.Wait
                       }
                   }, cancellationToken: cancellationToken);

            for (int i = 0; i < worker; i++)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var natsMsg in sub.Msgs.ReadAllAsync(cancellationToken))
                        {
                            if (natsMsg is { Data: not null })
                            {
                                var type = _types.GetOrAdd(natsMsg.Data.MessageType, _ => Type.GetType(natsMsg.Data.MessageType));

                                var message = (IEventMessage)MessagePackSerializer.Deserialize(type, natsMsg.Data.Message,
                                    MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Options);

                                MessageReceived?.Invoke(this, message);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "error at subscriber");
                    }
                }, cancellationToken);
            }
        }
    }

    private ValueTask ConnectionOnReconnectFailed(object sender, NatsEventArgs args)
    {
        Console.WriteLine(args.Message);
        return ValueTask.CompletedTask;
    }

    private ValueTask ConnectionOnMessageDropped(object sender, NatsMessageDroppedEventArgs args)
    {
        Console.WriteLine(args.Message);
        return ValueTask.CompletedTask;
    }

    private ValueTask ConnectionOnConnectionDisconnected(object sender, NatsEventArgs args)
    {
        Console.WriteLine(args.Message);
        return ValueTask.CompletedTask;
    }

    private ValueTask ConnectionOnConnectionOpened(object sender, NatsEventArgs args)
    {
        Console.WriteLine(args.Message);
        return ValueTask.CompletedTask;
    }

    public Task UnsubscribeAsync(string topic, CancellationToken cancellationToken)
    {
        if (_subscriptions.TryRemove(topic, out _))
        {
        }

        return Task.CompletedTask;
    }
}