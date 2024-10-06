using MessagePack;
using StackExchange.Redis;

namespace PriceEngine.Tools.StressTest.KeyDb;

public sealed class KeyDbPublisher : IMessagePublisher
{
    private readonly ISubscriber _publisher;

    public KeyDbPublisher(ILoggerFactory loggerFactory, KeyDbConfig config)
    {
        var connection = ConnectionMultiplexer.Connect(KeyDbConfigProvider.GetConfig(config.Endpoint, config.Password, loggerFactory));

        _publisher = connection.GetSubscriber();
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

        var bytes = MessagePackSerializer.Serialize(wrapper, MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Options);

        await _publisher.PublishAsync(new RedisChannel(data.Topic, RedisChannel.PatternMode.Auto), bytes, CommandFlags.FireAndForget);
    }
}