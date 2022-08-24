using AutoMapper;
using Confluent.Kafka;
using Grpc.Core;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using OrdersService.Data.Abstractions;
using OrdersService.Data.Repository;
using OrdersService.Models;
using OrdersService.Protos;
using OrdersService.Services.KafkaSettings;

namespace OrdersService.Services
{
    // Добавить обработку ошибок
    public class OrderServiceGrpc : OrderProcessing.OrderProcessingBase
    {
        public delegate void OrderCreated();

        public event OrderCreated onCreated;

        private readonly ILogger<OrderServiceGrpc> _logger;
        // Убрать репозитории
        private readonly IMongoRepository<ActiveSellOrder> _repositorySellOrder;
        private readonly IMongoRepository<ActiveBuyOrder> _repositoryBuyOrder;
        private readonly IMongoRepository<CompletedOrder> _repositoryCompleted;
        private readonly IMongoRepository<Order> _orderRepository;
        private readonly IMongoRepository<BestProductPrice> _productPriceRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<IKafkaSettings> _config;
        private readonly OrderOperationService _service;

        public OrderServiceGrpc(ILogger<OrderServiceGrpc> logger, 
                                      IMongoRepository<ActiveSellOrder> repositorySellOrder,
                                      IMongoRepository<ActiveBuyOrder> repositoryBuyOrder,
                                      IMongoRepository<CompletedOrder> repositoryCompleted,
                                      IMongoRepository<Order> orderRepository,
                                      IMongoRepository<BestProductPrice> productPriceRepository,
                                      IOptions<IKafkaSettings> kafkaSettings,
                                      OrderOperationService service,
                                      IMapper mapper)
        {
            _logger = logger;
            _repositorySellOrder = repositorySellOrder;
            _repositoryBuyOrder = repositoryBuyOrder;
            _repositoryCompleted = repositoryCompleted;
            _orderRepository = orderRepository;
            _productPriceRepository = productPriceRepository;
            _service = service;
            _mapper = mapper;

            // Переделать
            // Переместить создание индексов для коллекций 
            _repositorySellOrder.CreateIndexAsync();
            _repositoryBuyOrder.CreateIndexAsync();
            _repositoryCompleted.CreateIndexAsync();
            _orderRepository.CreateIndexAsync();
            _productPriceRepository.CreateIndexAsync();

            _config = kafkaSettings;
        }
        
        public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, 
            ServerCallContext context)
        {
            var orderData = _mapper.Map<Models.Messages.DataCreateOrder>(request.Data);

            var userId = orderData.UserId;
            var orderId = new ObjectId();

            var mainOrder = new Models.Order();
            mainOrder.Id = orderId;
            mainOrder.UserId = userId;
            mainOrder.OrderType = orderData.OrderType;
            mainOrder.ProductId = orderData.ProductId;
            mainOrder.ProductName = orderData.ProductName;
            mainOrder.Quantity = orderData.Quantity;
            mainOrder.Price = orderData.Price;

            await _orderRepository.InsertOneAsync(mainOrder);            

            if (orderData.OrderType == 0)
                CreateSellOrder(orderData, orderId);
            else
                CreateBuyOrder(orderData, orderId);

            return new CreateOrderResponse
            {
                Success = new SuccessResponse()
            };
        }

        // Переделать 
        private async void CreateSellOrder(Models.Messages.DataCreateOrder orderData, ObjectId orderId)
        {
            await Task.Run(async () =>
            {
                var sellOrder = new ActiveSellOrder();

                sellOrder.Id = new ObjectId(orderData.UserId);
                sellOrder.InTransaction = false;
                sellOrder.OrderId = orderId.ToString();

                await _repositorySellOrder.InsertOneAsync(sellOrder);

                var producer = new KafkaProducerService(_config);
                SellOrderCreated message = new SellOrderCreated()
                {
                    Id = orderData.ProductId,
                    Name = orderData.ProductName,
                    Quantity = orderData.Quantity,
                    Price = orderData.Price,
                    UserId = orderData.UserId
                };

                await producer.ProduceMessageAsync(message, "SellOrderCreated");

                var data = new DataCreateBestPrice(orderData.ProductId,
                                   orderData.ProductName,
                                   orderId.ToString(),
                                   orderData.Price,
                                   Models.Enums.PriceType.SellPrice);

                await _service.CreateBestPrice(data);
            });
        }

        // Переделать 
        private async void CreateBuyOrder(Models.Messages.DataCreateOrder orderData, ObjectId orderId)
        {
            await Task.Run(async () =>
            {
                var buyOrder = new ActiveBuyOrder();

                buyOrder.Id = new ObjectId(orderData.UserId);
                buyOrder.InTransaction = false;
                buyOrder.OrderId = orderId.ToString();

                await _repositoryBuyOrder.InsertOneAsync(buyOrder);

                var producer = new KafkaProducerService(_config);
                BuyOrderCreated message = new BuyOrderCreated()
                {
                    Id = orderData.ProductId,
                    Name = orderData.ProductName,
                    Quantity = orderData.Quantity,
                    Price = orderData.Price,
                    UserId = orderData.UserId
                };

                await producer.ProduceMessageAsync(message, "BuyOrderCreated");

                var data = new DataCreateBestPrice(orderData.ProductId,
                                                   orderData.ProductName,
                                                   orderId.ToString(),
                                                   orderData.Price,
                                                   Models.Enums.PriceType.BuyPrice);

                await _service.CreateBestPrice(data);
            });
        }

        // 2 метода выше вынести в один
    }
}
