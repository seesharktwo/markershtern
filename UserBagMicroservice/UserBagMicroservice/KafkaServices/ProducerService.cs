using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using UserBagMicroservice.Data.Kafka;
using UserBagMicroservice.Data.Settings;

namespace UserBagMicroservice.KafkaServices
{
    public class ProducerService<TKey, TValue> where TValue : IMessage
    {
        private readonly string _topicName;
        private readonly ProducerConfig _config;

        public ProducerService(string topicName, IOptions<KafkaSettings> config)
        {
            _topicName = topicName;
            _config = new ProducerConfig
            {
                BootstrapServers = config.Value.BootstrapServers
            };
        }

        public async Task SendMessage(Message<TKey, TValue> message)
        {
            using var producer = new ProducerBuilder<TKey, TValue>(_config).SetValueSerializer(new Serializer<TValue>()).Build();

            var response = await producer.ProduceAsync(
                          _topicName, new Message<TKey, TValue>
                          {
                              Headers = message.Headers,
                              Key = message.Key,
                              Value = message.Value
                          }
                         ); ; ;
        }
    }
}
