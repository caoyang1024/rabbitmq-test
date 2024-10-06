using System.Buffers;
using MessagePack;
using NATS.Client.Core;

namespace PriceEngine.Tools.StressTest.Nats;

public sealed class MessagePackDeserialize<T> : INatsDeserialize<T>
{
    public T Deserialize(in ReadOnlySequence<byte> buffer)
    {
        try
        {
            return MessagePackSerializer.Deserialize<T>(buffer, MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Options);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}