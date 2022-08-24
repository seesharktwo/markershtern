using AutoMapper;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using OrdersService.Data.Repository;
using OrdersService.Models;
using OrdersService.Models.Messages;
using OrdersService.Protos;
using OrdersService.Services.KafkaSettings;

namespace OrdersService.Services
{
    // Добавить обработку ошибок
    public class OrderOperationService
    {
        // Переделать
        // Убрать репозитории
        private readonly ILogger<OrderServiceGrpc> _logger;
        private readonly IMongoRepository<ActiveSellOrder> _repositorySellOrder;
        private readonly IMongoRepository<ActiveBuyOrder> _repositoryBuyOrder;
        private readonly IMongoRepository<CompletedOrder> _repositoryCompleted;
        private readonly IMongoRepository<Order> _orderRepository;
        private readonly IMongoRepository<BestProductPrice> _priceRepository;
        private readonly IOptions<IKafkaSettings> _config;
        private readonly IMapper _mapper;

        public OrderOperationService(ILogger<OrderServiceGrpc> logger,
                                      IMongoRepository<ActiveSellOrder> repositorySellOrder,
                                      IMongoRepository<ActiveBuyOrder> repositoryBuyOrder,
                                      IMongoRepository<CompletedOrder> repositoryCompleted,
                                      IMongoRepository<Order> orderRepository,
                                      IMongoRepository<BestProductPrice> priceRepository,
                                      IOptions<IKafkaSettings> settings,
                                      IMapper mapper)
        {
            _logger = logger;
            _repositorySellOrder = repositorySellOrder;
            _repositoryBuyOrder = repositoryBuyOrder;
            _repositoryCompleted = repositoryCompleted;
            _orderRepository = orderRepository; 
            _priceRepository = priceRepository;
            _mapper = mapper;

            _config = settings;
        } 

        public async Task CloseOrders(OrderCandidateOccuredProcessSuccess result)
        {
            var firstOrderId = result.OrderId;
            var secondOrderId = result.OrderIdSeller;

            Order firstOrder = _orderRepository.FindById(firstOrderId);
            Order secondOrder = _orderRepository.FindById(secondOrderId);

            var firstUserId = _orderRepository.FindById(firstOrderId).UserId;
            var secondUserId = _orderRepository.FindById(secondOrderId).UserId;

            // Убрать дублирование
            await _repositoryBuyOrder.DeleteOneAsync(filter =>
                    filter.Id == new ObjectId(firstUserId) &&
                    filter.OrderId == firstOrderId
                );

            // Убрать дублирование
            await _repositorySellOrder.DeleteOneAsync(filter =>
                    filter.Id == new ObjectId(secondUserId) &&
                    filter.OrderId == secondOrderId
                );

            var firstCompletedOrder = await CreateCompletedOrder(firstUserId, firstOrderId, secondUserId, secondOrderId);
            var secondCompletedOrder = await CreateCompletedOrder(secondUserId, secondOrderId, firstUserId, firstOrderId);

            await _repositoryCompleted.InsertManyAsync(new List<CompletedOrder> { 
                firstCompletedOrder, secondCompletedOrder 
            });
        }

        private async Task<CompletedOrder> CreateCompletedOrder(string firstUserId, 
            string firstOrderId, string secondUserId, string secondOrderId)
        {
            var completedOrder = new CompletedOrder();

            completedOrder.Id = new ObjectId(firstUserId);
            completedOrder.OrderId = firstOrderId;
            completedOrder.UserIdSecond = secondUserId;
            completedOrder.OrderIdSecond = secondOrderId;

            return completedOrder;
        }

        // Убрать дублирование
        public async Task DeleteOrdersSoldProducts(ProductSoldEvent message)
        {
            var userId = message.UserId;
            var orderId = message.OrderId;
            var productId = message.ProductId;
            var quantity = message.Quantity;

            var type = _orderRepository.FindById(orderId).OrderType;

            if (type == 0)
            {
                await _repositoryBuyOrder.DeleteManyAsync(
                        filter => filter.Id == new ObjectId(userId) &&
                        filter.OrderId == orderId
                    );

                await _orderRepository.DeleteManyAsync(
                        filter => filter.Id == new ObjectId(orderId) &&
                        filter.OrderType == Models.Enums.OrderType.SellOrder &&
                        filter.ProductId == productId &&
                        filter.Quantity > quantity
                    );
            }
        }

        // Убрать дублирование 
        public async Task DeleteOrdersRemovedProducts(ProductRemovedEvent message)
        {
            var userId = message.UserId;
            var productId = message.ProductId;

            IEnumerable<string> ordersId = _repositorySellOrder.FilterBy(
                    filter => filter.Id == new ObjectId(userId),
                    projection => projection.OrderId
                );

            foreach(var id in ordersId)
            {
                await _orderRepository.DeleteManyAsync(filter =>
                        filter.Id == new ObjectId(id) &&
                        filter.ProductId == productId 
                        && filter.OrderType == Models.Enums.OrderType.SellOrder
                    );

                await _repositorySellOrder.DeleteManyAsync(filter =>
                        filter.Id == new ObjectId(userId) &&
                        filter.OrderId == id
                    );
            }
        }

        // Переделать, переместить в OrderOperationService. Создать событие и подписаться
        public async Task CreateBestPrice(DataCreateBestPrice data)
        {
            var documentProductPrice = await _priceRepository.FindOneAsync(filter =>
                filter.Id == new ObjectId(data.ProductId));

            // Вывести в метод
            if (documentProductPrice is null)
            {
                var bestProductPrice = new BestProductPrice();

                bestProductPrice.Id = new ObjectId(data.ProductId);
                bestProductPrice.OrderId = data.OrderId;

                if (data.PriceType == Models.Enums.PriceType.SellPrice)
                    bestProductPrice.BestSellPrice = data.Price;
                else
                    bestProductPrice.BestBuyPrice = data.Price;

                await _priceRepository.InsertOneAsync(bestProductPrice);
            }
            else
            {
                // Вывести в метод
                var replaceDocument = new BestProductPrice();
                replaceDocument.Id = new ObjectId(data.ProductId);
                replaceDocument.OrderId = data.OrderId;

                if (data.PriceType == Models.Enums.PriceType.SellPrice)
                    replaceDocument.BestSellPrice = data.Price;
                else
                    replaceDocument.BestBuyPrice = data.Price;

                await _priceRepository.ReplaceOneAsync(replaceDocument);

                var producer = new KafkaProducerService(_config);

                var message = new Models.Messages.ProductPriceChanged();
                message.ProductId = data.ProductId;
                message.ProductName = data.ProductName;
                message.PriceType = data.PriceType;
                message.Price = data.Price;

                var priceChanged = _mapper.Map<Protos.ProductPriceChanged>(message);

                await producer.ProduceMessageAsync(priceChanged, "ProductPriceChanged");
            }
        }

        public async Task FindSimilarOrders(Models.Messages.DataCreateOrder data, ObjectId orderId)
        {
            var activeOrderId = orderId;
            var activeOrderType = data.OrderType;
            var activeProductId = data.ProductId;
            var activeProductName = data.ProductName;
            var activeQuantity = data.Quantity;
            var activePrice = data.Price;

            var ordersId = _repositorySellOrder.FilterBy(filter =>
                filter.InTransaction == false).Select(file => file.OrderId);

            // Разбить на отдельные выражения
            // читать невозможно
            var similarOrders = _orderRepository.FilterBy(filter =>
                                                          filter.OrderType != activeOrderType &&
                                                          filter.ProductId == activeProductId).Where(filter =>
                                                          {
                                                              if(filter.OrderType == Models.Enums.OrderType.BuyOrder) 
                                                                  return true;
                                                              
                                                          });


            // Продюсер отправляет в топик
            var producer = new KafkaProducerService(_config);

            var message = new OrderCandidateOccuredProcessEvent()
            {

            };
        }
    }
}
