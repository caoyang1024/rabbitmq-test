namespace PriceEngine.Tools.StressTest;

public sealed record StressTestPublisherConfig
{
    public int NumberOfThreads { get; init; }
    public int MessagesPerSecondPerThread { get; init; }
    public int DurationInSeconds { get; init; }
    public int MessageSize { get; init; }
}