using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Microsoft.Extensions.Options;
using UserBalanceMicroservice.CustomDeserializers;
using UserBalanceMicroservice.Configs;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Transact;
using OrderEvents;
using UserBalanceMicroservice;

namespace TransactService.Services
{

    public class KafkaConsumerService : BackgroundService
    {
        private readonly ConsumerConfig _config;
        private readonly TransactOperationService _service;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly TransactContext _context;

        private CancellationToken _cancellationToken;
        public KafkaConsumerService(IOptions<KafkaSettings> config,TransactContext context, TransactOperationService service, ILogger<KafkaConsumerService> logger)
        {
            _config = new ConsumerConfig
            {
                BootstrapServers = config.Value.BootstrapServers,
                GroupId = config.Value.GroupId,
                ClientId = Dns.GetHostName()
            };
            _service = service;
            _logger = logger;
            _context = context;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Task[] UnionTasks =  new Task[]
            {
                Task.Run(() => TransactionCanceledConsume(cancellationToken)),
                Task.Run(() => TransactionCompletedConsume(cancellationToken)),
                Task.Run(() => OrderClosedConsume(cancellationToken)),
                Task.Run(() => BalanceReplenishedConsume(cancellationToken)),
                Task.Run(() => SellOrderCreatedConsume(cancellationToken)),
                Task.Run(() => BuyOrderCreatedConsume(cancellationToken))
            };
            
            return Task.WhenAll(UnionTasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task TransactionCanceledConsume(CancellationToken cancellationToken)
        {
            using (var consumerBuilder = new ConsumerBuilder<Ignore, TransactionCanceled>(_config)
                .SetValueDeserializer(new ProtoDeserializer<TransactionCanceled>())
                .Build())
            {
                consumerBuilder.Subscribe("TransactionCanceled");
                var cancelToken = new CancellationTokenSource();

                while (!cancelToken.IsCancellationRequested)
                {
                    var consumer = consumerBuilder.Consume(cancelToken.Token);

                    var message = consumer.Message.Value;
                    try
                    {
                       //await _service.ExecuteTransact(message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"KafkaConsumerService Exception - {e.Message}");
                        continue;
                    }
                    //_logger.LogInformation($"UserBalanceMicroservice KafkaConsumerService Committed IdGlobalTransact - {message.IdGlobalTransact},IdUser - {message.IdUser}");
                }
            }
        }

        private async Task TransactionCompletedConsume(CancellationToken cancellationToken)
        {
            using (var consumerBuilder = new ConsumerBuilder<Ignore, TransactionCompleted>(_config)
                .SetValueDeserializer(new ProtoDeserializer<TransactionCompleted>())
                .Build())
            {
                consumerBuilder.Subscribe("TransactionCompleted");
                var cancelToken = new CancellationTokenSource();

                while (!cancelToken.IsCancellationRequested)
                {
                    var consumer = consumerBuilder.Consume(cancelToken.Token);

                    var message = consumer.Message.Value;
                    try
                    {
                        //await _service.ExecuteTransact(message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"KafkaConsumerService Exception - {e.Message}");
                        continue;
                    }
                    //_logger.LogInformation($"UserBalanceMicroservice KafkaConsumerService Committed IdGlobalTransact - {message.IdGlobalTransact},IdUser - {message.IdUser}");
                }
            }
        }

        private async Task OrderClosedConsume(CancellationToken cancellationToken)
        {
            using (var consumerBuilder = new ConsumerBuilder<Ignore, OrderClosed>(_config)
                .SetValueDeserializer(new ProtoDeserializer<OrderClosed>())
                .Build())
            {
                consumerBuilder.Subscribe("OrderClosed");
                var cancelToken = new CancellationTokenSource();

                while (!cancelToken.IsCancellationRequested)
                {
                    var consumer = consumerBuilder.Consume(cancelToken.Token);

                    var message = consumer.Message.Value;
                    try
                    {
                        await _service.ExecuteOrderClosed(message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"KafkaConsumerService Exception - {e.Message}");
                        continue;
                    }
                    _logger.LogInformation($"TransactService KafkaConsumerService Committed OrderClosed - {message.IdOrderBuyer},IdUser - {message.IdUserSeller}");
                }
            }
        }
        private async Task BalanceReplenishedConsume(CancellationToken cancellationToken)
        {
            using (var consumerBuilder = new ConsumerBuilder<Ignore, BalanceReplenished>(_config)
                .SetValueDeserializer(new ProtoDeserializer<BalanceReplenished>())
                .Build())
            {
                consumerBuilder.Subscribe("BalanceReplenished");
                var cancelToken = new CancellationTokenSource();

                while (!cancelToken.IsCancellationRequested)
                {
                    var consumer = consumerBuilder.Consume(cancelToken.Token);

                    var message = consumer.Message.Value;
                    try
                    {
                        await _service.ExecuteBalanceReplenished(message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"KafkaConsumerService Exception - {e.Message}");
                        continue;
                    }
                    //_logger.LogInformation($"UserBalanceMicroservice KafkaConsumerService Committed IdGlobalTransact - {message.IdGlobalTransact},IdUser - {message.IdUser}");
                }
            }
        }

        private async Task BuyOrderCreatedConsume(CancellationToken cancellationToken)
        {
            using (var consumerBuilder = new ConsumerBuilder<Ignore, BuyOrderCreated>(_config)
                .SetValueDeserializer(new ProtoDeserializer<BuyOrderCreated>())
                .Build())
            {
                consumerBuilder.Subscribe("BuyOrderCreated");
                var cancelToken = new CancellationTokenSource();

                while (!cancelToken.IsCancellationRequested)
                {
                    var consumer = consumerBuilder.Consume(cancelToken.Token);

                    var message = consumer.Message.Value;
                    try
                    {
                       await _context.CreateOrderValueAsync(message.Id, message.UserId, 0, message.Quanity, 0, "");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"KafkaConsumerService Exception - {e.Message}");
                        continue;
                    }
                    //_logger.LogInformation($"UserBalanceMicroservice KafkaConsumerService Committed IdGlobalTransact - {message.IdGlobalTransact},IdUser - {message.IdUser}");
                }
            }
        }

        private async Task SellOrderCreatedConsume(CancellationToken cancellationToken)
        {
            using (var consumerBuilder = new ConsumerBuilder<Ignore, SellOrderCreated>(_config)
                .SetValueDeserializer(new ProtoDeserializer<SellOrderCreated>())
                .Build())
            {
                consumerBuilder.Subscribe("SellOrderCreated");
                var cancelToken = new CancellationTokenSource();

                while (!cancelToken.IsCancellationRequested)
                {
                    var consumer = consumerBuilder.Consume(cancelToken.Token);

                    var message = consumer.Message.Value;
                    try
                    {
                        await _context.CreateOrderValueAsync(message.Id, message.UserId, 0, message.Quanity, 0, "");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"KafkaConsumerService Exception - {e.Message}");
                        continue;
                    }
                    //_logger.LogInformation($"UserBalanceMicroservice KafkaConsumerService Committed IdGlobalTransact - {message.IdGlobalTransact},IdUser - {message.IdUser}");
                }
            }
        }
    }
}
