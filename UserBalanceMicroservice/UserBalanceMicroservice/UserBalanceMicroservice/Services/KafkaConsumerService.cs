using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using eventList.CustomTypes;
using Microsoft.Extensions.Options;
using UserBalanceMicroservice.CustomDeserializers;
using UserBalanceMicroservice.Configs;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace UserBalanceMicroservice.Services
{

    public class KafkaConsumerService : BackgroundService
    {
        private readonly ConsumerConfig _config;
        private readonly BalanceOperationService _service;
        private readonly ILogger<KafkaConsumerService> _logger;

        private CancellationToken _cancellationToken;
        public KafkaConsumerService(IOptions<KafkaSettings> config, BalanceOperationService service, ILogger<KafkaConsumerService> logger)
        {
            _config = new ConsumerConfig
            {
                BootstrapServers = config.Value.BootstrapServers,
                GroupId = config.Value.GroupId,
                ClientId = Dns.GetHostName()
            };
            _service = service;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Task[] UnionTasks =  new Task[]
            {
                Task.Run(() => BalanceCommittedConsume(cancellationToken))
            };
            
            return Task.WhenAll(UnionTasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task BalanceCommittedConsume(CancellationToken cancellationToken)
        {
            using (var consumerBuilder = new ConsumerBuilder<Ignore, TransactionBalanceCommitted>(_config)
                .SetValueDeserializer(new ProtoDeserializer<TransactionBalanceCommitted>())
                .Build())
            {
                consumerBuilder.Subscribe("TransactionBalanceCommitted");
                var cancelToken = new CancellationTokenSource();

                while (!cancelToken.IsCancellationRequested)
                {
                    var consumer = consumerBuilder.Consume(cancelToken.Token);

                    var message = consumer.Message.Value;
                    try
                    {
                       await _service.ExecuteTransact(message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"UserBalanceMicroservice KafkaConsumerService Exception - {e.Message}");
                        continue;
                    }
                    _logger.LogInformation($"UserBalanceMicroservice KafkaConsumerService Committed IdGlobalTransact - {message.IdGlobalTransact},IdUser - {message.IdUser}");
                }
            }
        }
    }
}
