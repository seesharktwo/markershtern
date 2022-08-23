using Confluent.Kafka;
using Google.Protobuf;

namespace UserBagMicroservice.Data.Kafka
{
    public class Serializer<T> : ISerializer<T> where T : IMessage
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return data.ToByteArray();
        }
    }
}
