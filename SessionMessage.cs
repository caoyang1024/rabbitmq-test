namespace PriceEngine.Tools.StressTest;

public sealed class SessionMessage : IEventMessage
{
    public static string SessionTopic = nameof(SessionMessage);
    public string Topic => SessionTopic;
    public long EventSentTimeEpoch { get; set; }
    public long EventReceiveTimeEpoch { get; set; }

    public string Status { get; set; }
}