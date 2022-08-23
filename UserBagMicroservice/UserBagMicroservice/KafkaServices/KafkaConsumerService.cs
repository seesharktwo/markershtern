using Confluent.Kafka;
using eventList.CustomTypes;
using Microsoft.Extensions.Options;
using UserBagMicroservice.Data.Kafka;
using UserBagMicroservice.Data.Settings;
using UserBagMicroservice.Services;

namespace UserBagMicroservice.KafkaServices
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly UserBagOperationService _service;
        private readonly ConsumerConfig _config;

        public KafkaConsumerService(IOptions<KafkaSettings> config, UserBagOperationService service, ILogger<KafkaConsumerService> logger)
        {
            _config = new ConsumerConfig
            {
                BootstrapServers = config.Value.BootstrapServers,
                GroupId = config.Value.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _service = service;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task[] UnionTasks = new Task[]
            {
                Task.Run(() => ProductChangedConsumer(stoppingToken))
            };

            return Task.WhenAll(UnionTasks);
        }

        private async Task ProductChangedConsumer(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<Null, ProductChanged>(_config).SetValueDeserializer(new Deserializer<ProductChanged>()).Build();
            consumer.Subscribe("ProductChanged");
            CancellationTokenSource token = new();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var response = consumer.Consume(token.Token).Message.Value;
                    _logger.LogInformation("Event accepted: ProductChanged");
                    if (response != null)
                    {
                        await _service.ChangeProduct(response);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
