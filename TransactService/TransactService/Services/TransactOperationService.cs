using Microsoft.Extensions.Options;
using MoneyTypes;
using Transact;
using TransactService.Models;
using UserBalanceMicroservice;
using UserBalanceMicroservice.Configs;
using UserBalanceMicroservice.Services;

namespace TransactService.Services
{
    public class TransactOperationService
    {
        private readonly TransactContext _context;
        private readonly ILogger<TransactOperationService> _logger;
        private readonly IOptions<KafkaSettings> _config;
        private const int _maxCountWrite = 10;
        public TransactOperationService(TransactContext context, ILogger<TransactOperationService> logger, IOptions<KafkaSettings> config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public async Task ExecuteBalanceReplenished(BalanceReplenished balanceReplenished)
        {
            var globalTransact = await _context.GetAndCreateGlobalTransactAsync(balanceReplenished.Sum);

            if (await VirtualTransactionBalance(globalTransact, balanceReplenished.IdUserBuyer, balanceReplenished.IdUserBuyer, null, DecimalValue.ToDecimal(balanceReplenished.Sum)))
            {
                ProducerService producerService = new ProducerService(_config);
                TransactionBalanceCommitted UserSeller = new TransactionBalanceCommitted()
                {
                    IdGlobalTransact = globalTransact.Id,
                    IdOrder = string.Empty,
                    IdUser = balanceReplenished.IdUserBuyer,
                    Sum = balanceReplenished.Sum,
                    MODE = Operation.Addition,
                    TYPE = TransactionType.Action
                };
                await producerService.ProduceMessageAsync<TransactionBalanceCommitted>(UserSeller, "TransactionBalanceCommitted");
                await TransactionBalance(globalTransact, balanceReplenished.IdUserBuyer, balanceReplenished.IdUserBuyer, null, DecimalValue.ToDecimal(balanceReplenished.Sum));
                await _context.UpdateStatusGlobalTransactAsync(globalTransact.Id, Status.ACTION);
            }
            else
            {
                await RollbackTransactionBalance(globalTransact, balanceReplenished.IdUserBuyer, balanceReplenished.IdUserBuyer, null, DecimalValue.ToDecimal(balanceReplenished.Sum));
                await _context.UpdateStatusGlobalTransactAsync(globalTransact.Id, Status.ROLLBACK);
            }
        }

        public async Task ExecuteOrderClosed(OrderClosed orderClosed)
        {
            var globalTransact = await _context.GetAndCreateGlobalTransactAsync(orderClosed.Sum);

            var result = await TryVirtualExecute(orderClosed, globalTransact);

            if (result.All(x=>x))
            {
                await FixingTransact(orderClosed, globalTransact);
                await TryExecute(orderClosed, globalTransact); // поправить потом хз когда. Тут все плохо
                await _context.UpdateStatusGlobalTransactAsync(globalTransact.Id, Status.ACTION);
            }
            else
            {
                await RollbackTransact(orderClosed, globalTransact);
                await _context.UpdateStatusGlobalTransactAsync(globalTransact.Id, Status.ROLLBACK);
            }
        }

        private async Task RollbackTransact(OrderClosed orderClosed, GlobalTransact globalTransact)
        {
            await RollbackTransactionBalance(globalTransact, orderClosed.IdUserBuyer, orderClosed.IdUserBuyer, DecimalValue.ToDecimal(orderClosed.Sum), null);
            await RollbackTransactionBalance(globalTransact, orderClosed.IdUserSeller, orderClosed.IdUserSeller, null, DecimalValue.ToDecimal(orderClosed.Sum));

            await RollbackTransactionBriefcase(globalTransact, orderClosed.IdProduct, orderClosed.IdUserBuyer, null, orderClosed.CountProduct);
            await RollbackTransactionBriefcase(globalTransact, orderClosed.IdProduct, orderClosed.IdUserSeller, orderClosed.CountProduct, null);

            await RollbackTransactionOrder(globalTransact, orderClosed.IdOrderBuyer, orderClosed.IdUserBuyer, orderClosed.CountProduct, null);
            await RollbackTransactionOrder(globalTransact, orderClosed.IdOrderSeller, orderClosed.IdUserSeller, orderClosed.CountProduct, null);
        }


        private async Task FixingTransact(OrderClosed orderClosed, GlobalTransact globalTransact)
        {
            ProducerService producerService = new ProducerService(_config);

            TransactionBalanceCommitted UserBuyer = new TransactionBalanceCommitted() 
            { 
                IdGlobalTransact = globalTransact.Id,
                IdOrder = orderClosed.IdOrderBuyer,
                IdUser = orderClosed.IdUserBuyer,
                Sum = orderClosed.Sum,
                MODE = Operation.Subtract,
                TYPE = TransactionType.Action
            };
            await producerService.ProduceMessageAsync<TransactionBalanceCommitted>(UserBuyer, "TransactionBalanceCommitted");

            TransactionProductCommitted transactionSeller = new TransactionProductCommitted()
            {
                TYPE = TransactionType.Action,
                MODE = Operation.Subtract,
                Count = orderClosed.CountProduct,
                IdGlobalTransact = globalTransact.Id,
                IdOrder = orderClosed.IdOrderSeller,
                IdProduct = orderClosed.IdProduct,
                IdUser = orderClosed.IdUserSeller
            };
            await producerService.ProduceMessageAsync<TransactionProductCommitted>(transactionSeller, "TransactionProductCommitted");

            TransactionProductCommitted transactionBuyer = new TransactionProductCommitted()
            {
                TYPE = TransactionType.Action,
                MODE = Operation.Addition,  
                Count = orderClosed.CountProduct,
                IdGlobalTransact = globalTransact.Id,
                IdOrder = orderClosed.IdOrderBuyer,
                IdProduct = orderClosed.IdProduct,
                IdUser = orderClosed.IdUserBuyer
            };
            await producerService.ProduceMessageAsync<TransactionProductCommitted>(transactionBuyer, "TransactionProductCommitted");

            TransactionBalanceCommitted UserSeller = new TransactionBalanceCommitted()
            {
                IdGlobalTransact = globalTransact.Id,
                IdOrder = orderClosed.IdOrderSeller,
                IdUser = orderClosed.IdOrderSeller,
                Sum = orderClosed.Sum,
                MODE = Operation.Addition,
                TYPE = TransactionType.Action
            };
            await producerService.ProduceMessageAsync<TransactionBalanceCommitted>(UserSeller, "TransactionBalanceCommitted");
        }

        

        private async Task<List<bool>> TryVirtualExecute(OrderClosed orderClosed, GlobalTransact globalTransact )
        {
            List<bool> result = new List<bool>();
            result.Add(await VirtualTransactionBalance(globalTransact, orderClosed.IdUserBuyer, orderClosed.IdUserBuyer, DecimalValue.ToDecimal(orderClosed.Sum), null));
            result.Add(await VirtualTransactionBalance(globalTransact, orderClosed.IdUserSeller, orderClosed.IdUserSeller, null, DecimalValue.ToDecimal(orderClosed.Sum)));

            result.Add(await VirtualTransactionBriefcase(globalTransact, orderClosed.IdProduct, orderClosed.IdUserBuyer, null, orderClosed.CountProduct));
            result.Add(await VirtualTransactionBriefcase(globalTransact, orderClosed.IdProduct, orderClosed.IdUserSeller, orderClosed.CountProduct, null));

            result.Add(await VirtualTransactionOrder(globalTransact, orderClosed.IdOrderBuyer, orderClosed.IdUserBuyer, orderClosed.CountProduct, null));
            result.Add(await VirtualTransactionOrder(globalTransact, orderClosed.IdOrderSeller, orderClosed.IdUserSeller, orderClosed.CountProduct, null));

            return result;
        }

        private async Task<List<bool>> TryExecute(OrderClosed orderClosed, GlobalTransact globalTransact)
        {
            List<bool> result = new List<bool>();
            result.Add(await TransactionBalance(globalTransact, orderClosed.IdUserBuyer, orderClosed.IdUserBuyer, DecimalValue.ToDecimal(orderClosed.Sum), null));
            result.Add(await TransactionBalance(globalTransact, orderClosed.IdUserSeller, orderClosed.IdUserSeller, null, DecimalValue.ToDecimal(orderClosed.Sum)));

            result.Add(await TransactionBriefcase(globalTransact, orderClosed.IdProduct, orderClosed.IdUserBuyer, null, orderClosed.CountProduct));
            result.Add(await TransactionBriefcase(globalTransact, orderClosed.IdProduct, orderClosed.IdUserSeller, orderClosed.CountProduct, null));

            result.Add(await TransactionOrder(globalTransact, orderClosed.IdOrderBuyer, orderClosed.IdUserBuyer, orderClosed.CountProduct, null));
            result.Add(await TransactionOrder(globalTransact, orderClosed.IdOrderSeller, orderClosed.IdUserSeller, orderClosed.CountProduct, null));

            return result;
        }


        private async Task RollbackTransactionBalance(GlobalTransact globalTransact, string IdBalance, string UserId, decimal? credit, decimal? debit)
        {
            var transaction = await _context.GetBalanceTransactAsync(globalTransact.Id, IdBalance, UserId);

            if (transaction.Status != Status.PROVISORY)
            {
                return;
            }

            for (int count = 0; count < _maxCountWrite; count++)
            {
                var balance = await _context.GetBalanceValueAsync(IdBalance, UserId);
                bool result = false;
                if (credit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdBalance, UserId, balance.Credit - credit.Value, null, null, globalTransact.Id, balance.CountUpdate);
                }
                else if (debit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdBalance, UserId, null, debit.Value - balance.VirtualDebit, null, globalTransact.Id, balance.CountUpdate);
                }
                else
                {
                    throw new NotSupportedException();
                }
                if (result)
                {
                    await _context.UpdateStatusBalanceTransactAsync(globalTransact.Id, IdBalance, UserId, Status.ROLLBACK);
                    return;
                }
            }
        }

        private async Task RollbackTransactionBriefcase(GlobalTransact globalTransact, string IdProduct, string UserId, int? credit, int? debit)
        {
            var transaction = await _context.GetBriefcaseTransactAsync(globalTransact.Id, IdProduct, UserId);

            if (transaction.Status != Status.PROVISORY)
            {
                return;
            }
            for (int count = 0; count < _maxCountWrite; count++)
            {
                var briefcase = await _context.GetBriefcaseValueAsync(IdProduct, UserId);
                bool result = false;
                if (credit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdProduct, UserId, briefcase.Credit - credit.Value, null, null, globalTransact.Id, briefcase.CountUpdate);
                }
                else if (debit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdProduct, UserId, null, debit.Value - briefcase.VirtualDebit, null, globalTransact.Id, briefcase.CountUpdate);
                }
                else
                {
                    throw new NotSupportedException();
                }
                if (result)
                {
                    await _context.UpdateStatusBriefcaseTransactAsync(globalTransact.Id, IdProduct, UserId, Status.ROLLBACK);
                    return;
                }
            }
        }

        private async Task RollbackTransactionOrder(GlobalTransact globalTransact, string idObject, string UserId, int? credit, int? debit)
        {
            var transaction = await _context.GetOrderTransactAsync(globalTransact.Id, idObject, UserId);

            if (transaction.Status != Status.PROVISORY)
            {
                return;
            }
            for (int count = 0; count < _maxCountWrite; count++)
            {
                var briefcase = await _context.GetOrderValueAsync(idObject, UserId);
                bool result = false;
                if (credit.HasValue)
                {
                    result = await _context.UpdateOrderValueAsync(idObject, UserId, briefcase.Credit - credit.Value, null, null, globalTransact.Id, briefcase.CountUpdate);
                }
                else if (debit.HasValue)
                {
                    result = await _context.UpdateOrderValueAsync(idObject, UserId, null, debit.Value - briefcase.VirtualDebit, null, globalTransact.Id, briefcase.CountUpdate);
                }
                else
                {
                    throw new NotSupportedException();
                }
                if (result)
                {
                    await _context.UpdateStatusOrderTransactAsync(globalTransact.Id, idObject, UserId, Status.ROLLBACK);
                    return;
                }
            }
        }


        private async Task<bool> TransactionBalance(GlobalTransact globalTransact, string IdBalance, string UserId, decimal? credit, decimal? debit)
        {
            for (int count = 0; count < _maxCountWrite; count++)
            {
                var balance = await _context.GetBalanceValueAsync(IdBalance, UserId);
                bool result = false;
                if (credit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdBalance, UserId, balance.Credit - credit.Value, null, balance.Summ - credit.Value , globalTransact.Id, balance.CountUpdate);
                }
                else if (debit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdBalance, UserId, null, debit.Value - balance.VirtualDebit, balance.Summ + debit.Value, globalTransact.Id, balance.CountUpdate);
                }
                else
                {
                    throw new NotSupportedException();
                }
                if (result)
                {
                    await _context.UpdateStatusBalanceTransactAsync(globalTransact.Id, IdBalance, UserId, Status.ACTION);
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> TransactionBriefcase(GlobalTransact globalTransact, string IdProduct, string UserId, int? credit, int? debit)
        {
            for (int count = 0; count < _maxCountWrite; count++)
            {
                var briefcase = await _context.GetBriefcaseValueAsync(IdProduct, UserId);
                bool result = false;
                if (credit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdProduct, UserId, briefcase.Credit - credit.Value, null, briefcase.Quanity - credit.Value, globalTransact.Id, briefcase.CountUpdate);

                }
                else if (debit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdProduct, UserId, null, debit.Value - briefcase.VirtualDebit, briefcase.Quanity + debit.Value, globalTransact.Id, briefcase.CountUpdate);
                }
                else
                {
                    throw new NotSupportedException();
                }
                if (result)
                {
                    await _context.UpdateStatusBriefcaseTransactAsync(globalTransact.Id, IdProduct, UserId, Status.ACTION);
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> TransactionOrder(GlobalTransact globalTransact, string idObject, string UserId, int? credit, int? debit)
        {
            for (int count = 0; count < _maxCountWrite; count++)
            {
                var order = await _context.GetOrderValueAsync(idObject, UserId);
                bool result = false;
                if (credit.HasValue)
                {
                    result = await _context.UpdateOrderValueAsync(idObject, UserId, order.Credit - credit.Value, null, order.Quanity - credit.Value, globalTransact.Id, order.CountUpdate);
                }
                else if (debit.HasValue)
                {
                    result = await _context.UpdateOrderValueAsync(idObject, UserId, null, debit.Value - order.VirtualDebit, order.Quanity + debit.Value, globalTransact.Id, order.CountUpdate);
                }
                else
                {
                    throw new NotSupportedException();
                }
                if (result)
                {
                    await _context.UpdateStatusOrderTransactAsync(globalTransact.Id, idObject, UserId, Status.ACTION);
                    return true;
                }
            }
            return false;
        }


        private async Task<bool> VirtualTransactionBalance(GlobalTransact globalTransact, string IdBalance, string UserId,decimal? credit, decimal? debit )
        {
            await _context.CreateBalanceTransactAsync(globalTransact.Value, globalTransact.Id, IdBalance, UserId);
            await _context.CreateBalanceValueAsync(IdBalance, UserId, 0, 0, 0, string.Empty);
            for (int count = 0; count< _maxCountWrite; count++)
            {
                var balance = await _context.GetBalanceValueAsync(IdBalance, UserId);
                
                bool result = false;
                if (credit.HasValue )
                {
                    if ((balance.Credit + credit.Value) <= balance.Summ)
                    {
                        result = await _context.UpdateBalanceValueAsync(IdBalance, UserId, balance.Credit + credit.Value, null, null, globalTransact.Id, balance.CountUpdate);

                    }
                    else
                    {
                        return false;
                    }
                }
                else if (debit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdBalance, UserId, null, debit.Value + balance.VirtualDebit, null, globalTransact.Id, balance.CountUpdate);
                }
                else
                {
                    throw new NotSupportedException();
                }
                if (result)
                {
                    await _context.UpdateStatusBalanceTransactAsync(globalTransact.Id, IdBalance, UserId, Status.PROVISORY);
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> VirtualTransactionBriefcase(GlobalTransact globalTransact, string IdProduct, string UserId, int? credit, int? debit)
        {
            await _context.CreateBriefcaseTransactAsync((int)globalTransact.Value, globalTransact.Id, IdProduct, UserId);
            await _context.CreateBriefcaseValueAsync(IdProduct, UserId, 0, 0, 0, string.Empty);
            for (int count = 0; count < _maxCountWrite; count++)
            {
                var briefcase = await _context.GetBriefcaseValueAsync(IdProduct, UserId);
                bool result = false;
                if (credit.HasValue)
                {
                    //Проверки нет, так как отсутствует событие. 
                    result = await _context.UpdateBalanceValueAsync(IdProduct, UserId, briefcase.Credit + credit.Value, null, null, globalTransact.Id, briefcase.CountUpdate);
                }
                else if (debit.HasValue)
                {
                    result = await _context.UpdateBalanceValueAsync(IdProduct, UserId, null, debit.Value + briefcase.VirtualDebit, null, globalTransact.Id, briefcase.CountUpdate);
                }
                else
                {
                    throw new NotSupportedException();
                }
                if (result)
                {
                    await _context.UpdateStatusBriefcaseTransactAsync(globalTransact.Id, IdProduct, UserId, Status.PROVISORY);
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> VirtualTransactionOrder(GlobalTransact globalTransact, string idObject, string UserId, int? credit, int? debit)
        {
            await _context.CreateOrderTransactAsync((int)globalTransact.Value, globalTransact.Id, idObject, UserId);
            //await _context.CreateOrderValueAsync(idObject, UserId, 0, 0, 0, string.Empty);
            for (int count = 0; count < _maxCountWrite; count++)
            {
                var briefcase = await _context.GetOrderValueAsync(idObject, UserId);
                bool result = false;
                if (credit.HasValue)
                {
                    if ((briefcase.Credit + credit.Value) <= briefcase.Quanity)
                    {
                        result = await _context.UpdateOrderValueAsync(idObject, UserId, briefcase.Credit + credit.Value, null, null, globalTransact.Id, briefcase.CountUpdate);
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (debit.HasValue)
                {
                    result = await _context.UpdateOrderValueAsync(idObject, UserId, null, debit.Value + briefcase.VirtualDebit, null, globalTransact.Id, briefcase.CountUpdate);
                }
                else
                {
                    throw new NotSupportedException();
                }
                if (result)
                {
                    await _context.UpdateStatusOrderTransactAsync(globalTransact.Id, idObject, UserId, Status.PROVISORY);
                    return true;
                }
            }
            return false;
        }
    }
}
