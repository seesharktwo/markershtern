using Confluent.Kafka;
using Google.Protobuf;

namespace UserBagMicroservice.Data.Kafka
{
    public class Deserializer<T> : IDeserializer<T> where T : IMessage<T>, new()
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return new MessageParser<T>(() => new T()).ParseFrom(data);
        }
    }
}
