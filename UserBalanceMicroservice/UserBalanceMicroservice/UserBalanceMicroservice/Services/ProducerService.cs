using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using System.Net;
using UserBalanceMicroservice.Configs;
using UserBalanceMicroservice.ProtosServices;

namespace UserBalanceMicroservice.Services
{
    public class ProducerService<T>  where T : IMessage<T>, new()
    {
        private readonly ProducerConfig _producerConfig;
        public ProducerService(IOptions<KafkaSettings> config)
        {
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = config.Value.BootstrapServers,
                ClientId = Dns.GetHostName()
            };
        }

        public async Task ProduceMessageAsync(T message,string topic)
        {
            var producer = new ProducerBuilder<Ignore, T>(_producerConfig);

            producer.SetValueSerializer(new ProducerSerializer<T>());
            var producerBuild = producer.Build();
            await producerBuild.ProduceAsync(topic, new Message<Ignore, T>
            {
                Value = message
            });

            producerBuild?.Dispose();
        }
    }
}
