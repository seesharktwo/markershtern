using Confluent.Kafka;
using Microsoft.Extensions.Options;
using UserBagMicroservice.Data.Kafka;
using UserBagMicroservice.Data.Settings;
using UserBagMicroservice.Services;
using Briefcase;

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
                Task.Run(() => TransactionProductCommittedConsumer(stoppingToken))
            };

            return Task.WhenAll(UnionTasks);
        }

        private async Task TransactionProductCommittedConsumer(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<Null, TransactionProductCommitted>(_config).SetValueDeserializer(new Deserializer<TransactionProductCommitted>()).Build();
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
