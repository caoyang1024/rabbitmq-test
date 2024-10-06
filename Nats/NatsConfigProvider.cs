using System.Text;
using System.Threading.Channels;
using NATS.Client.Core;

namespace PriceEngine.Tools.StressTest.Nats;

public static class NatsConfigProvider
{
    public static NatsOpts GetConfig(string url, string name, ILoggerFactory loggerFactory)
    {
        return new NatsOpts
        {
            Url = url,
            Name = name,
            Echo = true,
            Verbose = true,
            Headers = true,
            AuthOpts = NatsAuthOpts.Default,
            TlsOpts = NatsTlsOpts.Default,
            WebSocketOpts = NatsWebSocketOpts.Default,
            SerializerRegistry = new MessageSerializerRegistry(),
            LoggerFactory = loggerFactory,
            WriterBufferSize = 65536,
            ReaderBufferSize = 65536,
            UseThreadPoolCallback = false,
            // InboxPrefix = null,
            NoRandomize = false,
            PingInterval = TimeSpan.FromMinutes(1),
            MaxPingOut = 3,
            ReconnectWaitMin = TimeSpan.FromSeconds(2),
            ReconnectJitter = TimeSpan.FromMilliseconds(100),
            ConnectTimeout = TimeSpan.FromSeconds(3),
            ObjectPoolSize = 256,
            RequestTimeout = TimeSpan.FromSeconds(5),
            CommandTimeout = TimeSpan.FromSeconds(5),
            SubscriptionCleanUpInterval = TimeSpan.FromMinutes(5),
            HeaderEncoding = Encoding.ASCII,
            SubjectEncoding = Encoding.ASCII,
            WaitUntilSent = false,
            MaxReconnectRetry = -1,
            ReconnectWaitMax = TimeSpan.FromSeconds(5),
            IgnoreAuthErrorAbort = false,
            SubPendingChannelCapacity = 1024,
            SubPendingChannelFullMode = BoundedChannelFullMode.DropOldest
        };
    }
}