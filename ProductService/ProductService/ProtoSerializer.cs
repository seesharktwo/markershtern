using Confluent.Kafka;
using Google.Protobuf;
using OrderEvents;

namespace ProducerService.CustomDeserializers
{
    public class ProtoSerializer<T> : ISerializer<T>
        where T : IMessage<T>, new()
    {

        public byte[] Serialize(T data, SerializationContext context)
        {
            if (data is null)
                return null;
            var res = data.ToByteArray();
            return res;
        }
    }
}
