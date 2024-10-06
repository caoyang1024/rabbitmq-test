using PriceEngine.Tools.StressTest.KeyDb;
using PriceEngine.Tools.StressTest.Nats;

namespace PriceEngine.Tools.StressTest;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        if (builder.Configuration.GetSection("StressTestPublisherConfig").Exists() == false)
        {
            throw new InvalidConfigurationException("config missing");
        }

        var config = builder.Configuration.GetSection("StressTestPublisherConfig").Get<StressTestPublisherConfig>();

        builder.Services.AddSingleton(config);

        if (builder.Configuration.GetSection("KeyDbConfig").Exists() == false)
        {
            throw new InvalidConfigurationException("keydb config missing");
        }

        var keyDbConfig = builder.Configuration.GetSection("KeyDbConfig").Get<KeyDbConfig>();

        builder.Services.AddSingleton(keyDbConfig);

        if (builder.Configuration.GetSection("NatsConfig").Exists() == false)
        {
            throw new InvalidConfigurationException("nats config missing");
        }

        var natsConfig = builder.Configuration.GetSection("NatsConfig").Get<NatsConfig>();

        builder.Services.AddSingleton(natsConfig);

        builder.Logging.AddPriceEngineLoggingService(builder.Configuration);

        builder.Services.AddSingleton<IMessagePublisher, NatsPublisher>();
        builder.Services.AddSingleton<IMessageSubscriber, NatsSubscriber>();

        //builder.Services.AddSingleton<IMessagePublisher, KeyDbPublisher>();
        //builder.Services.AddSingleton<IMessageSubscriber, KeyDbSubscriber>();

        builder.Services.AddHostedService<StressTestSubscriber>();

        var host = builder.Build();

        await host.RunAsync();
    }
}