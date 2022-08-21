using Confluent.Kafka;
using EnumList.CustomTypes;
using eventList.CustomTypes;
using Microsoft.Extensions.Options;
using Protos.CustomTypes;
using UserBalanceMicroservice;
using UserBalanceMicroservice.Configs;
using UserBalanceMicroservice.Services;
using MoneyTypes;

namespace UserBalanceMicroservice.Services
{
    public class BalanceOperationService
    {
        private readonly BalanceContext _context;
        private readonly ILogger<BalanceOperationService> _logger;
        private readonly IOptions<KafkaSettings> _config;
        public BalanceOperationService(BalanceContext context, ILogger<BalanceOperationService> logger, IOptions<KafkaSettings> config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public async Task ExecuteTransact(TransactionBalanceCommitted transaction)
        {
            if (transaction.TYPE == TransactionType.Action)
            {
                await Executer(transaction);
            }
            else if (transaction.TYPE == TransactionType.Rollback)
            {
                await ExecuteTransactROLLBACK(transaction);
            }
            else
            {
                _logger.LogError($"UserBalanceMicroservice BalanceService - not valid TransactionType {transaction.IdGlobalTransact}:id,{transaction.IdOrder}:IdOrder,{transaction.IdUser}:IdUser");
            }
        }

        private async Task Executer(TransactionBalanceCommitted transaction)
        {
            bool isAdd = false;
            if (transaction.MODE == Operation.Addition)
            {
                isAdd = true;
            }
            else if (transaction.MODE == Operation.Subtract)
            {
                isAdd = false;
            }
            else
            {
                _logger.LogError($"UserBalanceMicroservice BalanceService - not valid Operation {transaction.IdGlobalTransact}:id,{transaction.IdOrder}:IdOrder,{transaction.IdUser}:IdUser");
                return;
            }

            var result = await _context.ChangeBalanceAsync(transaction.IdUser, transaction.Sum, isAdd, transaction.IdGlobalTransact);

            await Feedback(result,transaction);
        }

        private async Task Feedback(bool result, TransactionBalanceCommitted transaction)
        {
            var balance = await _context.GetBalanceAsync(transaction.IdUser);

            SourceEventTransaction source;
            if (transaction.TYPE == TransactionType.Action)
            {
                source = SourceEventTransaction.BalanceSubtractImmediate;
            }
            else
            {
                source = SourceEventTransaction.BalanceSubtractRollback;
            }
            

            if (result)
            {
                var producerCompleted = new ProducerService<TransactionCompleted>(_config);
                TransactionCompleted message = new TransactionCompleted()
                {
                    IdGlobalTransact = transaction.IdGlobalTransact,
                    IdObject = transaction.IdUser,
                    SOURCE = source,
                    Quanity = DecimalValue.FromDecimal(balance.Money)
                };
                await producerCompleted.ProduceMessageAsync(message, "TransactionCompleted");
                _logger.LogInformation($"UserBalanceMicroservice BalanceService - TransactionCompleted IdGlobalTransact - {message.IdGlobalTransact}, IdObject -{message.IdObject}");
            }
            else
            {
                var producerCompleted = new ProducerService<TransactionCanceled>(_config);
                TransactionCanceled message = new TransactionCanceled()
                {
                    IdGlobalTransact = transaction.IdGlobalTransact,
                    SOURCE = source
                };
                await producerCompleted.ProduceMessageAsync(message, "TransactionCanceled");
                _logger.LogInformation($"UserBalanceMicroservice BalanceService - TransactionCanceled IdGlobalTransact - {message.IdGlobalTransact}, SOURCE -{message.SOURCE.ToString()}");
            }
        }

        private async Task ExecuteTransactROLLBACK(TransactionBalanceCommitted transaction)
        {
            var temp = transaction.Clone();
            temp.IdGlobalTransact = "";
            await Executer(temp);
        }
    }
}
