using StackExchange.Redis;

namespace PriceEngine.Tools.StressTest.KeyDb;

public static class KeyDbConfigProvider
{
    public static ConfigurationOptions GetConfig(string endpoint, string password, ILoggerFactory loggerFactory)
    {
        return new ConfigurationOptions
        {
            EndPoints = { endpoint },
            Password = password,
            AbortOnConnectFail = false,
            KeepAlive = 30,
            LoggerFactory = loggerFactory,
            // The number of times to repeat connect attempts during initial Connect, set 5 times here
            ConnectRetry = 5,
            // Timeout (ms) for connect operations, set 5 seconds here
            ConnectTimeout = 5000,
            // Determines how often a multiplexer will try to reconnect after a failure, set 15 seconds here
            ReconnectRetryPolicy = new LinearRetry(15 * 1000),
            // Determines how commands will be queued (or not) during a disconnect, for sending when it’s available again
            BacklogPolicy = BacklogPolicy.FailFast,
            // Allows running the heartbeat more often which importantly includes timeout evaluation for async commands.
            HeartbeatInterval = TimeSpan.FromSeconds(10),
        };
    }
}