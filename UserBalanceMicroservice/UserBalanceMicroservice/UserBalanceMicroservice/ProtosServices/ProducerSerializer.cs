using Confluent.Kafka;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserBalanceMicroservice.ProtosServices
{
    public class ProducerSerializer<T>  : ISerializer<T> where T : IMessage
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return ((IMessage)data).ToByteArray();
        }
    }
}
