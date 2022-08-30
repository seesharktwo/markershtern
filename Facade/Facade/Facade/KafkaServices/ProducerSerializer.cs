using Confluent.Kafka;
using Google.Protobuf;

namespace Facade.KafkaServices
{
    public class ProducerSerializer<T> : ISerializer<T> 
        where T : IMessage
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return ((IMessage)data).ToByteArray();
        }
    }
}
