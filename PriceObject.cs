namespace PriceEngine.Tools.StressTest;

public sealed class PriceObject : IEventMessage
{
    public string CorrelationId { get; set; }

    public DateTimeOffset LPOutTime { get; set; }

    public DateTimeOffset SourceInTime { get; set; }

    public DateTimeOffset ServiceInTime { get; set; }

    public DateTimeOffset ServiceOutTime { get; set; }

    public int MarketId { get; set; }

    public string Symbol { get; set; }

    public string FeederSource { get; set; }

    public decimal Bid { get; set; }

    public decimal? BidVolume { get; set; }

    public decimal Ask { get; set; }

    public decimal? AskVolume { get; set; }

    public decimal Mid { get; set; }

    public int? DepthMarketId { get; set; }

    public string Model { get; set; }

    public string FullName => $"{nameof(PriceObject)}:{MarketId}:{Symbol}:{FeederSource}";
    public string Topic => $"{nameof(PriceObject)}.{MarketId}";
    public long EventSentTimeEpoch { get; set; }
    public long EventReceiveTimeEpoch { get; set; }
}