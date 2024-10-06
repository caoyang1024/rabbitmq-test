using NATS.Client.Core;

namespace PriceEngine.Tools.StressTest.Nats;

public sealed class MessageSerializerRegistry : INatsSerializerRegistry
{
    public INatsSerialize<T> GetSerializer<T>()
    {
        return new MessagePackSerialize<T>();
    }

    public INatsDeserialize<T> GetDeserializer<T>()
    {
        return new MessagePackDeserialize<T>();
    }
}