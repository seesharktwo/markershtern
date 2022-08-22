using Confluent.Kafka;
using Google.Protobuf;
using OrderEvents;

namespace ProducerService.CustomDeserializers
{
    public class ProtoDeserializer<T> : IDeserializer<T>
        where T : IMessage<T>, new()
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull)
                return default(T);

            MessageParser<T> parser = new MessageParser<T>(() => new T());
            var result = parser.ParseFrom(data);
            return result;

        }
    }
}
