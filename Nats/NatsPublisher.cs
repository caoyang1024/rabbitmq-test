using MessagePack;
using NATS.Client.Core;

namespace PriceEngine.Tools.StressTest.Nats;

public sealed class NatsPublisher : IMessagePublisher
{
    private readonly NatsConnection _connection;

    public NatsPublisher(ILoggerFactory loggerFactory, NatsConfig config)
    {
        _connection = new(
            NatsConfigProvider.GetConfig(
                config.Endpoint, $"{nameof(NatsPublisher)}_{Guid.NewGuid():D}", loggerFactory));

        _connection.ConnectionOpened += ConnectionOnConnectionOpened;
        _connection.ConnectionDisconnected += ConnectionOnConnectionDisconnected;
        _connection.MessageDropped += ConnectionOnMessageDropped;
        _connection.ReconnectFailed += ConnectionOnReconnectFailed;
    }

    public async Task PublishAsync(IEventMessage data, CancellationToken cancellationToken)
    {
        var msgType = data.GetType();
        string typeName = msgType.FullName;

        MessageWrapper wrapper = new()
        {
            MessageType = typeName,
            Message = MessagePackSerializer.Serialize(msgType, data, MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Options)
        };

        await _connection.PublishAsync(data.Topic, wrapper, cancellationToken: cancellationToken);
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
}