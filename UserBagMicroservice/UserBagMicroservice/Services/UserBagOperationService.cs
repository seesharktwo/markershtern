using UserBagMicroservice.Models;
using Microsoft.Extensions.Options;
using UserBagMicroservice.Data.Settings;
using UserBagMicroservice.Data.Repository;
using Confluent.Kafka;
using MoneyTypes;
using UserBagMicroservice.KafkaServices;
using MongoDB.Bson;
using Briefcase;

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

        public async Task ChangeProduct(TransactionProductCommitted response)
        {           
            try
            {
                var userBag = await _userBagRepository.FindOrCreateByIdAsync(new UserBag { Id = new ObjectId(response.IdUser) });

                if(response.MODE == Operation.Addition)
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
                _logger.LogInformation($"TransactionCompleted event dispatched\n\t{response.IdProduct}");
               
            }
            catch (Exception e)
            {
                var producer = new ProducerService<Null, TransactionCanceled>("TransactionCanceled", _config);
                await producer.SendMessage(GetTransactionCanceledMessage(response));
                _logger.LogError($"TransactionCanceled event dispatched\n\t{e.Message}");
            }
        }

        private void AddProduct(UserBag userBag, TransactionProductCommitted response)
        {
            var product = userBag.Products.FirstOrDefault(x => x.Id.ToString() == response.IdProduct);

            if (product is null)
            {
                userBag.Products.Add(new UserBagProduct { Id = ObjectId.Parse(response.IdProduct)});
                product = userBag.Products.First(x => x.Id.ToString() == response.IdProduct);
            }

            CheckService.CheckDublicateTransaction(product.TransactionId, response.IdGlobalTransact);

            product.Quantity += response.Count;
            product.TransactionId = response.IdGlobalTransact;
        }

        private void SubtractProduct(UserBag userBag, TransactionProductCommitted response)
        {
            var product = userBag.Products.FirstOrDefault(x => x.Id.ToString() == response.IdProduct);

            CheckService.CheckUserProductOnNull(product);
            CheckService.CheckDublicateTransaction(product.TransactionId, response.IdGlobalTransact);
            CheckService.CheckProductOnQuantity(product.Quantity, response.Count);

            product.Quantity -= response.Count;
            product.TransactionId = response.IdGlobalTransact;

            if (product.Quantity == 0)
            {
                userBag.Products.Remove(product);
            }
        }

        private SourceEventTransaction GetSourceEventTransaction(TransactionProductCommitted response)
        { 
            if (response.TYPE == TransactionType.Action)
            {
                if (response.MODE == Operation.Addition)
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
                if (response.MODE == Operation.Addition)
                {
                    return SourceEventTransaction.ProductBriefcaseAdditionRollback;
                }
                else
                {
                    return SourceEventTransaction.ProductBriefcaseSubtractRollback;
                }
            }
        }

        private Message<Null, TransactionCompleted> GetTransactionCompletedMessage(TransactionProductCommitted response)
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

        private Message<Null, TransactionCanceled> GetTransactionCanceledMessage(TransactionProductCommitted response)
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