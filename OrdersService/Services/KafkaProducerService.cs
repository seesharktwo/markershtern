using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using OrdersService.Deserializer;
using OrdersService.Services.KafkaSettings;
using System.Net;

namespace OrdersService.Services
{
    public class KafkaProducerService
    {
        private readonly ProducerConfig _producerConfig;
        public KafkaProducerService(IOptions<IKafkaSettings> config)
        {
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = config.Value.BootstrapServers,
                ClientId = Dns.GetHostName()
            };
        }

        public async Task ProduceMessageAsync<T>(T message, string topic)
            where T : IMessage<T>, new()
        {
            var producer = new ProducerBuilder<Ignore, T>(_producerConfig);

            producer.SetValueSerializer(new ProtoSerializer<T>());
            using (var producerBuild = producer.Build())
            {
                await producerBuild.ProduceAsync(topic, new Message<Ignore, T>
                {
                    Value = message
                });
            }
        }
    }
}
