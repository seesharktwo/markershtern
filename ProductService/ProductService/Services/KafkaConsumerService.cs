using Confluent.Kafka;
using Microsoft.Extensions.Options;
using ProductService.Configs;
using ProductService.Protos.Events;
using System.Diagnostics;
using System.Text.Json;

namespace ProductService.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ConsumerConfig _config;

        private readonly ProductService _service;

        public KafkaConsumerService(IOptions<KafkaConsumerSettings> config, ProductService service)
        {
            this._config = new ConsumerConfig
            {
                BootstrapServers = config.Value.BootstrapServers,
                GroupId = config.Value.GroupId
            };
            _service = service;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Task UnionTask = Task.Run(() =>
            {
                BuyOrderCreatedConsume(cancellationToken);
                SellOrderCreatedConsume(cancellationToken);
                ProductPriceChangedConsume(cancellationToken);
            });
            return UnionTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task BuyOrderCreatedConsume(CancellationToken cancellationToken)
        {
            try
            {
                using (var consumerBuilder = new ConsumerBuilder
                <Ignore, string>(_config).Build())
                {
                    consumerBuilder.Subscribe("BuyOrderCreated");
                    var cancelToken = new CancellationTokenSource();
                    try
                    {

                        while (true)
                        {
                            var consumer = consumerBuilder.Consume
                                (cancelToken.Token);
                            var newOrder = JsonSerializer.Deserialize
                                <BuyOrderCreated>
                                    (consumer.Message.Value);
                            _service.ProcessingBuyOrder(newOrder);
                            
                            Debug.WriteLine($"Processing Product Name: {newOrder.Name}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        consumerBuilder.Close();
                    }
                    catch (ConsumeException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        consumerBuilder.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task SellOrderCreatedConsume(CancellationToken cancellationToken)
        {
            try
            {
                using (var consumerBuilder = new ConsumerBuilder
                <Ignore, string>(_config).Build())
                {
                    consumerBuilder.Subscribe("SellOrderCreated");
                    var cancelToken = new CancellationTokenSource();
                    try
                    {

                        while (true)
                        {
                            var consumer = consumerBuilder.Consume
                                (cancelToken.Token);
                            var newOrder = JsonSerializer.Deserialize
                                <SellOrderCreated>
                                    (consumer.Message.Value);
                            _service.ProcessingSellOrder(newOrder);
                            Debug.WriteLine($"Processing Product Name: {newOrder.Name}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        consumerBuilder.Close();
                    }
                    catch (ConsumeException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        consumerBuilder.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task ProductPriceChangedConsume(CancellationToken cancellationToken)
        {
            try
            {
                using (var consumerBuilder = new ConsumerBuilder
                <Ignore, string>(_config).Build())
                {
                    consumerBuilder.Subscribe("SellOrderCreated");
                    var cancelToken = new CancellationTokenSource();
                    try
                    {

                        while (true)
                        {
                            var consumer = consumerBuilder.Consume
                                (cancelToken.Token);
                            var newOrder = JsonSerializer.Deserialize
                                <ProductPriceChanged>
                                    (consumer.Message.Value);
                            _service.ProcessingPriceChangedEvent(newOrder);
                            Debug.WriteLine($"Processing Product Name: {newOrder.Name}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        consumerBuilder.Close();
                    }
                    catch (ConsumeException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        consumerBuilder.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
