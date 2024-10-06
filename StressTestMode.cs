namespace PriceEngine.Tools.StressTest;

public class StressTestMode
{
    // KeyDb or Nats
    public string Target { get; set; }

    // Publisher or Subscriber
    public string Mode { get; set; }
}