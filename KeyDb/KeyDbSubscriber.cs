using System.Collections.Concurrent;
using MessagePack;
using StackExchange.Redis;

namespace PriceEngine.Tools.StressTest.KeyDb;

public sealed class KeyDbSubscriber : IMessageSubscriber
{
    private readonly ISubscriber _subscriber;
    private readonly ConcurrentDictionary<string, Type> _types = new();

    public KeyDbSubscriber(ILoggerFactory loggerFactory, KeyDbConfig config)
    {
        var connection = ConnectionMultiplexer.Connect(KeyDbConfigProvider.GetConfig(config.Endpoint, config.Password, loggerFactory));

        _subscriber = connection.GetSubscriber();
    }

    public async Task SubscribeAsync(string topic, int worker = 1, CancellationToken cancellationToken = default)
    {
        await _subscriber.SubscribeAsync(new RedisChannel(topic, RedisChannel.PatternMode.Auto), Handler);
    }

    private void Handler(RedisChannel channel, RedisValue value)
    {
        var wrapper = MessagePackSerializer.Deserialize<MessageWrapper>(value, MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Options);

        var type = _types.GetOrAdd(wrapper.MessageType, _ => Type.GetType(wrapper.MessageType));

        var message = (IEventMessage)MessagePackSerializer.Deserialize(type, wrapper.Message,
             MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Options);

        MessageReceived?.Invoke(this, message);
    }

    public async Task UnsubscribeAsync(string topic, CancellationToken cancellationToken)
    {
        await _subscriber.UnsubscribeAsync(new RedisChannel(topic, RedisChannel.PatternMode.Auto), Handler);
    }

    public event EventHandler<IEventMessage> MessageReceived;
}