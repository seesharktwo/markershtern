using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using UserBagMicroservice.Data.Repository;
using UserBagMicroservice.Data.Settings;
using UserBagMicroservice.Protos;

namespace UserBagMicroservice.Services
{


    public class KafkaConsumerService : BackgroundService
    {
        private readonly MongoRepository<Models.UserBag> _userBagRepository;
        private ILogger<KafkaConsumerService> _logger;
        private readonly ConsumerConfig _config;

        public KafkaConsumerService(IOptions<KafkaConsumerSettings> config, MongoRepository<Models.UserBag> userBagRepository, ILogger<KafkaConsumerService> logger)
        {
            _config = new ConsumerConfig
            {   
                BootstrapServers = config.Value.BootstrapServers,
                GroupId = config.Value.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _userBagRepository = userBagRepository;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(async () => await UserRegisteredConsumer(stoppingToken));
            Task.Run(async () => await OrderCompletedConsumer(stoppingToken));
            Task.Run(async () => await ProductSoldConsumer(stoppingToken));

            return Task.CompletedTask;
        }

        private async Task UserRegisteredConsumer(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<Null, UserRegisteredEvent>(_config).SetValueDeserializer(new Deserializer<UserRegisteredEvent>()).Build();

            consumer.Subscribe("RegistrationEvents");

            CancellationTokenSource token = new();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var response = consumer.Consume(token.Token);

                    _logger.Log(LogLevel.Information, "Принято событие: Пользователь зарегестрирован");

                    if (response.Message.Value != null)
                    {

                        if (_userBagRepository.FindById(response.Message.Value.UserId) != null)
                        {
                            _logger.Log( LogLevel.Error, $"Пользователь({response.Message.Value.UserId}) существует");
                            continue;  
                        }

                        await _userBagRepository.InsertOneAsync(new Models.UserBag() { Id = MongoDB.Bson.ObjectId.Parse(response.Message.Value.UserId), Products = new List<Models.Product>() });
                        _logger.Log(LogLevel.Information, $"Пользователь({response.Message.Value.UserId}) добавлен");
                    }

                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e.Message);
            }
        }

        private async Task OrderCompletedConsumer(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<Null, OrderCompletedEvent>(_config).SetValueDeserializer(new Deserializer<OrderCompletedEvent>()).Build();

            consumer.Subscribe("OrderCompletedEvents");

            CancellationTokenSource token = new();

            try
            {
                while (!token.IsCancellationRequested)
                {

                    var response = consumer.Consume(token.Token);

                    _logger.Log(LogLevel.Information, "Принято событие: Сделка завершена");
 

                    if (response.Message.Value != null)
                    {

                        var user1 = await _userBagRepository.FindByIdAsync(response.Message.Value.UserIdFrom);
                        var user2 = await _userBagRepository.FindByIdAsync(response.Message.Value.UserIdTo);

                        var user1Product = user1.Products.FirstOrDefault(product => product.Id == MongoDB.Bson.ObjectId.Parse(response.Message.Value.ProductId));
                        var user2Product = user2.Products.FirstOrDefault(product => product.Id == MongoDB.Bson.ObjectId.Parse(response.Message.Value.ProductId));

                        if(user2Product == null)
                        {
                            user2Product = (Models.Product)user1Product.Clone();
                            user2Product.Quantity = 0;
                            user2.Products.Add(user2Product);
                        }

                        user1Product.Quantity -= response.Message.Value.Quantity;
                        user2Product.Quantity += response.Message.Value.Quantity;

                        if (user1Product.Quantity == 0)
                        {
                            user1.Products.Remove(user1Product);
                        }
                            
                        await _userBagRepository.ReplaceOneAsync(user1);
                        await _userBagRepository.ReplaceOneAsync(user2);

                        //Отправляем событие Briefcase_ProductQuantityDecreaseEvent для user1
                        _logger.Log(LogLevel.Information, $"Количество товара изменено.\n" +
                            $"      Пользователь({user1.Id}) продал {user1Product.Name}({user1Product.Id}) в размере {response.Message.Value.Quantity}ед\n" +
                            $"      Пользователь({user2.Id}) купил {user1Product.Name}({user2Product.Id}) в размере {response.Message.Value.Quantity}ед");
                    }

                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e.Message);
            }
        }

        private async Task ProductSoldConsumer(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<Null, Order_ProductSoldEvent>(_config).SetValueDeserializer(new Deserializer<Order_ProductSoldEvent>()).Build();

            consumer.Subscribe("ProductSoldEvents");

            CancellationTokenSource token = new();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var response = consumer.Consume(token.Token);

                    _logger.Log(LogLevel.Information, "Принято событие: Сделка создана");

                    if (response.Message.Value != null)
                    {

                        var user = await _userBagRepository.FindByIdAsync(response.Message.Value.UserId);
                        var userProduct = user.Products.First(product => product.Id == MongoDB.Bson.ObjectId.Parse(response.Message.Value.ProductId));

                        var logResult = $"Проверка количества {userProduct.Name}({userProduct.Id}) у пользователя({user.Id})\n" +
                            $"      Требуется {response.Message.Value.Quantity}ед\tИмеется {userProduct.Quantity}ед";

                        if (userProduct.Quantity < response.Message.Value.Quantity)
                        {
                            //Отправляем событие Briefcase_ProductSoldEventResponse с отрицательным результатом 
                            _logger.Log(LogLevel.Error, logResult);
                        }
                        else
                        {
                            _logger.Log(LogLevel.Information, logResult);
                            //Отправляем событие Briefcase_ProductSoldEventResponse с положительным результатом 
                        }
                    }

                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e.Message);
            }
        }

        public class Deserializer<T> : IDeserializer<T> where T : IMessage<T>, new()
        {
            public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
            {
                return new MessageParser<T>(() => new T()).ParseFrom(data);
            }
        }

        public class Serializer<T> : ISerializer<T> where T : IMessage
        {
            public byte[] Serialize(T data, SerializationContext context)
            {
                return data.ToByteArray();
            }
        }

    }
}
