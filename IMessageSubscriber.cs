namespace PriceEngine.Tools.StressTest;

public interface IMessageSubscriber
{
    public Task SubscribeAsync(string topic, int worker = 1, CancellationToken cancellationToken = default);

    public Task UnsubscribeAsync(string topic, CancellationToken cancellationToken = default);

    public event EventHandler<IEventMessage> MessageReceived;
}