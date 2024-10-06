namespace PriceEngine.Tools.StressTest;

public interface IMessagePublisher
{
    public Task PublishAsync(IEventMessage data, CancellationToken cancellationToken);
}