namespace PriceEngine.Tools.StressTest;

public interface IEventMessage
{
    public string Topic { get; }
    public long EventSentTimeEpoch { get; set; }
    public long EventReceiveTimeEpoch { get; set; }
}