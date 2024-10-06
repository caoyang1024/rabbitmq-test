using Serilog;

namespace PriceEngine.Tools.StressTest;

public static class LogExtension
{
    public static void AddPriceEngineLoggingService(this ILoggingBuilder builder, IConfiguration configuration)
    {
        builder.ClearProviders();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        builder.AddSerilog(dispose: true);
    }
}