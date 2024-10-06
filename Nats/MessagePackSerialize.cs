using System.Buffers;
using NATS.Client.Core;

namespace PriceEngine.Tools.StressTest.Nats;

public sealed class MessagePackSerialize<T> : INatsSerialize<T>
{
    public void Serialize(IBufferWriter<byte> bufferWriter, T value)
    {
        try
        {
            MessagePack.MessagePackSerializer.Serialize(value.GetType(), bufferWriter, value, MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Options);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}