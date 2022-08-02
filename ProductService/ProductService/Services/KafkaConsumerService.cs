using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Microsoft.Extensions.Options;
using ProducerService.CustomDeserializers;
using ProductService.Configs;
using ProductService.Protos.Events;
using System.Diagnostics;
using System.Net;
using System.Text.Json;


namespace ProductService.Services
{
    /// <summary>
    /// service is listening topics in kafka
    /// </summary>
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ConsumerConfig _config;

        private readonly ProductService _service;
        private readonly ILogger<KafkaConsumerService> _logger;

        public KafkaConsumerService(IOptions<KafkaConsumerSettings> config, ProductService service, ILogger<KafkaConsumerService> logger)
        {
            this._config = new ConsumerConfig
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
                Task.Run(() => BuyOrderCreatedConsume(cancellationToken)),
                Task.Run(() => SellOrderCreatedConsume(cancellationToken)),
                Task.Run(() => ProductPriceChangedConsume(cancellationToken))
            };
            
            return Task.WhenAll(UnionTasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// task listens BuyOrderCreated topic and sends to productService
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task BuyOrderCreatedConsume(CancellationToken cancellationToken)
        {
            try
            {
                using (var consumerBuilder = new ConsumerBuilder<Ignore, BuyOrderCreated>(_config)
                .SetValueDeserializer(new ProtoDeserializer<BuyOrderCreated>())
                .Build())
                {
                    consumerBuilder.Subscribe("BuyOrderCreated");
                    var cancelToken = new CancellationTokenSource();
                    try
                    {

                        while (true)
                        {
                            var consumer = consumerBuilder.Consume(cancelToken.Token);

                            var message = consumer.Message.Value;
                            try
                            {
                                await _service.ProcessingBuyOrder(message);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Получено неверное сообщение: {ex.Message}");
                                continue;
                            }
                            _logger.LogInformation($"Получено сообщение покупки товара: {message.Name}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    finally
                    {
                        consumerBuilder.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
         }
        /// <summary>
        /// task listens SellOrderCreated topic and sends to productService
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SellOrderCreatedConsume(CancellationToken cancellationToken)
        {
            try
            {
                using (var consumerBuilder = new ConsumerBuilder
                <Ignore, SellOrderCreated>(_config)
                .SetValueDeserializer(new ProtoDeserializer<SellOrderCreated>())
                .Build())
                {
                    consumerBuilder.Subscribe("SellOrderCreated");
                    var cancelToken = new CancellationTokenSource();
                    try
                    {

                        while (true)
                        {
                            var consumer = consumerBuilder.Consume
                                (cancelToken.Token);
                            var message = consumer.Message.Value;
                            try
                            {
                                await _service.ProcessingSellOrder(message);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Получено неверное сообщение: {ex.Message}");
                                continue;
                            }
                            _logger.LogInformation($"Получено сообщение продажи товара: {message.Name}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    finally
                    {
                        consumerBuilder.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
        /// <summary>
        /// task listens ProductPriceChanged topic and sends to productService
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ProductPriceChangedConsume(CancellationToken cancellationToken)
        {
            try
            {
                using (var consumerBuilder = new ConsumerBuilder
                <Ignore, ProductPriceChanged>(_config)
                .SetValueDeserializer(new ProtoDeserializer<ProductPriceChanged>())
                .Build())
                {
                    consumerBuilder.Subscribe("ProductPriceChanged");
                    var cancelToken = new CancellationTokenSource();
                    try
                    {

                        while (true)
                        {
                            var consumer = consumerBuilder.Consume
                                (cancelToken.Token);
                            var message = consumer.Message.Value;
                            try
                            {
                                await _service.ProcessingPriceChangedEvent(message);
                            }
                            catch(Exception ex)
                            {
                                _logger.LogWarning($"Получено неверное сообщение: {ex.Message}");
                                continue;
                            }
                            _logger.LogInformation($"Получено сообщение изменения лучшей цены товара: {message.Name}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    finally
                    {
                        consumerBuilder.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
