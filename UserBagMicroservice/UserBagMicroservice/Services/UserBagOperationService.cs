using UserBagMicroservice.Models;
using Microsoft.Extensions.Options;
using UserBagMicroservice.Data.Settings;
using UserBagMicroservice.Data.Repository;
using Confluent.Kafka;
using EnumList.CustomTypes;
using eventList.CustomTypes;
using MoneyTypes;
using UserBagMicroservice.KafkaServices;

namespace UserBagMicroservice.Services
{
    public class UserBagOperationService
    {
        private readonly IOptions<KafkaSettings> _config;
        private readonly ILogger<UserBagOperationService> _logger;
        private readonly IMongoRepository<UserBag> _userBagRepository;

        public UserBagOperationService(IOptions<KafkaSettings> config, ILogger<UserBagOperationService> logger, MongoRepository<Models.UserBag> userBagRepository)
        {
            _config = config;
            _logger = logger;
            _userBagRepository = userBagRepository;
        }

        public async Task ChangeProduct(ProductChanged response)
        {
            var userBag = await _userBagRepository.FindByIdAsync(response.IdUser);   
            try
            {
                CheckService.CheckUserBagOnNull(userBag);

                if(response.Mode == Operation.Addition)
                {
                    AddProduct(userBag, response);            
                }
                else
                {
                    SubtractProduct(userBag, response);
                }

                await _userBagRepository.ReplaceOneAsync(userBag);
                var producer = new ProducerService<Null, TransactionCompleted>("TransactionCompleted", _config);
                await producer.SendMessage(GetTransactionCompletedMessage(response));
                _logger.LogInformation("Product was changed");
            }
            catch (Exception e)
            {
                var producer = new ProducerService<Null, TransactionCanceled>("TransactionCanceled", _config);
                await producer.SendMessage(GetTransactionCanceledMessage(response));
                _logger.LogError(e.Message);
            }
        }

        private void AddProduct(UserBag userBag, ProductChanged response)
        {
            var product = userBag.Products.FirstOrDefault(x => x.Id.ToString() == response.IdProduct);

            if (product is null)
            {
                userBag.Products.Add(new UserBagProduct { Id = MongoDB.Bson.ObjectId.Parse(response.IdProduct), Quantity = 0 });
                product = userBag.Products.FirstOrDefault(x => x.Id.ToString() == response.IdProduct);
            }

            CheckService.CheckDublicateTransaction(product.TransactionId, response.IdGlobalTransact);
            product.Quantity += response.Count;
            product.TransactionId = response.IdGlobalTransact;
        }

        private void SubtractProduct(UserBag userBag, ProductChanged response)
        {
            var product = userBag.Products.FirstOrDefault(x => x.Id.ToString() == response.IdProduct);
            CheckService.CheckProductOnNull(product);
            CheckService.CheckDublicateTransaction(product.TransactionId, response.IdGlobalTransact);
            CheckService.CheckProductOnQuantity(product.Quantity, response.Count);
            product.Quantity -= response.Count;
            product.TransactionId = response.IdGlobalTransact;

            if (product.Quantity == 0)
            {
                userBag.Products.Remove(product);
            }
        }

        private SourceEventTransaction GetSourceEventTransaction(ProductChanged response)
        { 
            if (response.Type == TransactionType.Action)
            {
                if (response.Mode == Operation.Addition)
                {
                    return SourceEventTransaction.ProductBriefcaseAdditionAction;
                }
                else
                {
                    return SourceEventTransaction.ProductBriefcaseSubtractAction;
                }
            }
            else
            {
                if (response.Mode == Operation.Addition)
                {
                    return SourceEventTransaction.ProductBriefcaseAdditionRollback;
                }
                else
                {
                    return SourceEventTransaction.ProductBriefcaseSubtractRollback;
                }
            }
        }

        private Message<Null, TransactionCompleted> GetTransactionCompletedMessage(ProductChanged response)
        {
            return new Message<Null, TransactionCompleted>
            {
                Value = new TransactionCompleted
                {
                    IdGlobalTransact = response.IdGlobalTransact,
                    IdObject = response.IdProduct,
                    Quanity = DecimalValue.FromDecimal(response.Count),
                    SOURCE = GetSourceEventTransaction(response)
                }
            };
        }

        private Message<Null, TransactionCanceled> GetTransactionCanceledMessage(ProductChanged response)
        {
            return new Message<Null, TransactionCanceled>
            {
                Value = new TransactionCanceled
                {
                    IdGlobalTransact = response.IdGlobalTransact,         
                    SOURCE = GetSourceEventTransaction(response)
                }
            };
        }

    }
}