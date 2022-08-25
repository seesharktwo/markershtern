﻿using Confluent.Kafka;
using Facade.Configs;
using Facade.ProtosServices;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using System.Net;
using System.Threading.Tasks;

namespace Facade.Services
{
    public class ProducerService
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

        public async Task ProduceMessageAsync<T>(T message, string topic)
            where T : IMessage<T>, new()
        {
            var producer = new ProducerBuilder<Null, T>(_producerConfig);

            producer.SetValueSerializer(new KafkaServices.ProducerSerializer<T>());
            var producerBuild = producer.Build();
            await producerBuild.ProduceAsync(topic, new Message<Null, T>
            {
                Value = message
            });

            producerBuild?.Dispose();
        }
    }
}