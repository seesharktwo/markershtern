
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Transactions;
using TransactService.Models;
using UserBalanceMicroservice.Configs;
namespace UserBalanceMicroservice
{
    public class TransactContext
    {
        private readonly IMongoDatabase _mongoDatabase;

        private const string _balanceTransactCollection = "BalanceTransact";
        private const string _briefcaseTransactCollection = "BriefcaseTransact";
        private const string _orderTransactCollection = "OrderTransact";
        private const string _globalTransactCollection = "GlobalTransact"; 
        private const string _balanceValueCollection = "BalanceValue";
        private const string _briefcaseValueCollection = "BriefcaseValue"; 
        private const string _orderValueCollection = "OrderValue"; 
        public TransactContext(
            IOptions<DatabaseSettings> bookStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                bookStoreDatabaseSettings.Value.ConnectionString);

            _mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

        }

        public async Task CreateBalanceTransactAsync(decimal value,string idTransact, string idBalance, string userId,Status status = Status.Nan)
        {
            var filter = Builders<BalanceTransact>.Filter
                .Where(x => x.IdBalance == idBalance && x.UserId == userId && x.IdTransact == idTransact);

            var temp = await _mongoDatabase.GetCollection<BalanceTransact>(_balanceTransactCollection)
                .Find(filter)
                .FirstOrDefaultAsync();

            if (temp != null)
            {
                return;
            }

            await _mongoDatabase.GetCollection<BalanceTransact>(_balanceTransactCollection)
                    .InsertOneAsync(new BalanceTransact() 
                    { 
                        Date = DateTime.Now,
                        IdTransact = idTransact,
                        IdBalance = idBalance,
                        UserId = userId,
                        Value = value,
                        Status = status
                    });
        }

        public async Task<BalanceTransact> GetBalanceTransactAsync(string idTransact, string idBalance, string userId)
        {
            var filter = Builders<BalanceTransact>.Filter
                .Where(x => x.IdBalance == idBalance && x.UserId == userId && x.IdTransact == idTransact);

            return await _mongoDatabase.GetCollection<BalanceTransact>(_balanceTransactCollection)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateStatusBalanceTransactAsync(string idTransact, string idBalance, string userId, Status status)
        {
            var filter = Builders<BalanceTransact>.Filter
                .Where(x => x.IdBalance == idBalance && x.UserId == userId && x.IdTransact == idTransact);

            var update = Builders<BalanceTransact>.Update.Set(x => x.Status, status);

            await _mongoDatabase.GetCollection<BalanceTransact>(_balanceTransactCollection)
                .UpdateOneAsync(filter, update);
        }


        public async Task CreateBriefcaseTransactAsync(int value, string idTransact, string idProduct, string userId, Status status = Status.Nan)
        {
            var filter = Builders<BriefcaseTransact>.Filter
                .Where(x => x.IdProduct == idProduct && x.UserId == userId && x.IdTransact == idTransact);

            var temp = await _mongoDatabase.GetCollection<BriefcaseTransact>(_briefcaseTransactCollection)
                .Find(filter)
                .FirstOrDefaultAsync();

            if (temp != null)
            {
                return;
            }

            await _mongoDatabase.GetCollection<BriefcaseTransact>(_briefcaseTransactCollection)
                    .InsertOneAsync(new BriefcaseTransact()
                    {
                        Date = DateTime.Now,
                        IdTransact = idTransact,
                        IdProduct = idProduct,
                        UserId = userId,
                        Value = value,
                        Status = status
                    });
        }

        public async Task<BriefcaseTransact> GetBriefcaseTransactAsync(string idTransact, string idProduct, string userId)
        {
            var filter = Builders<BriefcaseTransact>.Filter
                .Where(x => x.IdProduct == idProduct && x.UserId == userId && x.IdTransact == idTransact);

            return await _mongoDatabase.GetCollection<BriefcaseTransact>(_briefcaseTransactCollection)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateStatusBriefcaseTransactAsync(string idTransact, string idProduct, string userId, Status status)
        {
            var filter = Builders<BriefcaseTransact>.Filter
                .Where(x => x.IdProduct == idProduct && x.UserId == userId && x.IdTransact == idTransact);

            var update = Builders<BriefcaseTransact>.Update.Set(x => x.Status, status);

            await _mongoDatabase.GetCollection<BriefcaseTransact>(_briefcaseTransactCollection)
                .UpdateOneAsync(filter, update);
        }


        public async Task CreateOrderTransactAsync(int value, string idTransact, string idOrder, string userId, Status status = Status.Nan)
        {
            var filter = Builders<OrderTransact>.Filter
                .Where(x => x.IdOrder == idOrder && x.UserId == userId && x.IdTransact == idTransact);

            var temp = await _mongoDatabase.GetCollection<OrderTransact>(_orderTransactCollection)
                .Find(filter)
                .FirstOrDefaultAsync();

            if (temp != null)
            {
                return;
            }

            await _mongoDatabase.GetCollection<OrderTransact>(_orderTransactCollection)
                    .InsertOneAsync(new OrderTransact()
                    {
                        Date = DateTime.Now,
                        IdTransact = idTransact,
                        IdOrder = idOrder,
                        UserId = userId,
                        Value = value,
                        Status = status
                    });
        }

        public async Task<OrderTransact> GetOrderTransactAsync(string idTransact, string idOrder, string userId)
        {
            var filter = Builders<OrderTransact>.Filter
                .Where(x => x.IdOrder == idOrder && x.UserId == userId && x.IdTransact == idTransact);

            return await _mongoDatabase.GetCollection<OrderTransact>(_orderTransactCollection)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateStatusOrderTransactAsync(string idTransact, string idOrder, string userId, Status status)
        {
            var filter = Builders<OrderTransact>.Filter
                .Where(x => x.IdOrder == idOrder && x.UserId == userId && x.IdTransact == idTransact);

            var update = Builders<OrderTransact>.Update.Set(x => x.Status, status);

            await _mongoDatabase.GetCollection<OrderTransact>(_orderTransactCollection)
                .UpdateOneAsync(filter, update);
        }


        public async Task<GlobalTransact> GetAndCreateGlobalTransactAsync(decimal value, Status status = Status.Nan)
        {
            var temp = new GlobalTransact()
            {
                Date = DateTime.Now,
                Value = value,
                Status = status
            };
            await _mongoDatabase.GetCollection<GlobalTransact>(_globalTransactCollection)
                    .InsertOneAsync(temp);

            return temp;
        }

        public async Task<GlobalTransact> GetGlobalTransactAsync(string id)
        {
            var filter = Builders<GlobalTransact>.Filter
                .Where(x => x.Id == id);

            return await _mongoDatabase.GetCollection<GlobalTransact>(_globalTransactCollection)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateStatusGlobalTransactAsync(string idTransact, Status status)
        {
            var filter = Builders<GlobalTransact>.Filter
                .Where(x => x.Id == idTransact);

            var update = Builders<GlobalTransact>.Update.Set(x => x.Status, status);

            await _mongoDatabase.GetCollection<GlobalTransact>(_globalTransactCollection)
                .UpdateOneAsync(filter, update);
        }

        public async Task CreateBalanceValueAsync(string idBalance, string userId,decimal credit, decimal summ, decimal virtualDebit, string lastIdTransact)
        {
            var filter = Builders<BalanceValue>.Filter
                .Where(x =>x.IdBalance == idBalance && x.UserId == userId);

            var temp = await _mongoDatabase.GetCollection<BalanceValue>(_balanceValueCollection)
                .Find(filter)
                .FirstOrDefaultAsync();

            if (temp != null)
            {
                return;
            }

            var value = new BalanceValue()
            {
                IdBalance = idBalance,
                UserId = userId,
                Credit = credit,
                Summ = summ,
                VirtualDebit = virtualDebit,
                LastIdTransact = lastIdTransact,
                CountUpdate = 0
            };

            await _mongoDatabase.GetCollection<BalanceValue>(_balanceValueCollection)
                    .InsertOneAsync(value);
        }

        public async Task<BalanceValue> GetBalanceValueAsync(string idBalance, string userId)
        {
            var filter = Builders<BalanceValue>.Filter
                .Where(x => x.IdBalance == idBalance && x.UserId == userId);

            return await _mongoDatabase.GetCollection<BalanceValue>(_balanceValueCollection)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateBalanceValueAsync(string idBalance, string userId, decimal? credit, decimal? virtualDebit, decimal? summ, string lastIdTransact,ulong countUpdate)
        {
            var filter = Builders<BalanceValue>.Filter
                .Where(x => x.IdBalance == idBalance 
                && x.UserId == userId
                && x.CountUpdate == countUpdate );


            var update = Builders<BalanceValue>.Update.Set(x => x.LastIdTransact, lastIdTransact);
            update.AddToSet("CountUpdate", countUpdate+1);
            if (credit.HasValue)
            {
                update.AddToSet("Credit", credit.Value);
            }
            if (virtualDebit.HasValue)
            {
                update.AddToSet("VirtualDebit", virtualDebit.Value);
            }
            if (summ.HasValue)
            {
                update.AddToSet("Summ", summ.Value);
            }

            var result = await _mongoDatabase.GetCollection<BalanceValue>(_balanceValueCollection)
                .FindOneAndUpdateAsync(filter, update);
            return result != null;
        }

        //

        public async Task CreateBriefcaseValueAsync(string idProduct, string userId, int credit, int quanity, int virtualDebit, string lastIdTransact)
        {
            var filter = Builders<BriefcaseValue>.Filter
                .Where(x => x.IdProduct == idProduct && x.UserId == userId);

            var temp = await _mongoDatabase.GetCollection<BriefcaseValue>(_briefcaseValueCollection)
                .Find(filter)
                .FirstOrDefaultAsync();

            if (temp != null)
            {
                return;
            }

            var value = new BriefcaseValue()
            {
                IdProduct = idProduct,
                UserId = userId,
                Credit = credit,
                Quanity = quanity,
                VirtualDebit = virtualDebit,
                LastIdTransact = lastIdTransact,
                CountUpdate = 0
            };

            await _mongoDatabase.GetCollection<BriefcaseValue>(_briefcaseValueCollection)
                    .InsertOneAsync(value);
        }

        public async Task<BriefcaseValue> GetBriefcaseValueAsync(string idProduct, string userId)
        {
            var filter = Builders<BriefcaseValue>.Filter
                .Where(x => x.IdProduct == idProduct && x.UserId == userId);

            return await _mongoDatabase.GetCollection<BriefcaseValue>(_briefcaseValueCollection)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateBriefcaseValueAsync(string idProduct, string userId, int? credit, int? virtualDebit, int? quanity, string lastIdTransact,ulong countUpdate)
        {
            var filter = Builders<BriefcaseValue>.Filter
                .Where(x => x.IdProduct == idProduct 
                && x.UserId == userId
                && x.CountUpdate == countUpdate);


            var update = Builders<BriefcaseValue>.Update.Set(x => x.LastIdTransact, lastIdTransact);
            update.AddToSet("CountUpdate", countUpdate + 1);
            if (credit.HasValue)
            {
                update.AddToSet("Credit", credit.Value);
            }
            if (virtualDebit.HasValue)
            {
                update.AddToSet("VirtualDebit", virtualDebit.Value);
            }
            if (quanity.HasValue)
            {
                update.AddToSet("Quanity", quanity.Value);
            }

            var result = await _mongoDatabase.GetCollection<BriefcaseValue>(_briefcaseValueCollection)
                .FindOneAndUpdateAsync(filter, update);
            return result != null;
        }

        //OrderValue

        public async Task CreateOrderValueAsync(string idObject, string userId, int credit, int quanity, int virtualDebit, string lastIdTransact)
        {
            var filter = Builders<OrderValue>.Filter
                .Where(x => x.IdObject == idObject && x.UserId == userId);

            var temp = await _mongoDatabase.GetCollection<OrderValue>(_orderValueCollection)
                .Find(filter)
                .FirstOrDefaultAsync();

            if (temp != null)
            {
                return;
            }

            var value = new OrderValue()
            {
                IdObject = idObject,
                UserId = userId,
                Credit = credit,
                Quanity = quanity,
                VirtualDebit = virtualDebit,
                LastIdTransact = lastIdTransact,
                CountUpdate = 0
            };

            await _mongoDatabase.GetCollection<OrderValue>(_orderValueCollection)
                    .InsertOneAsync(value);
        }

        public async Task<OrderValue> GetOrderValueAsync(string idObject, string userId)
        {
            var filter = Builders<OrderValue>.Filter
                .Where(x => x.IdObject == idObject && x.UserId == userId);

            return await _mongoDatabase.GetCollection<OrderValue>(_orderValueCollection)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateOrderValueAsync(string idObject, string userId, int? credit, int? virtualDebit, int? quanity, string lastIdTransact, ulong countUpdate)
        {
            var filter = Builders<OrderValue>.Filter
                .Where(x => x.IdObject == idObject 
                && x.UserId == userId
                && x.CountUpdate == countUpdate);


            var update = Builders<OrderValue>.Update.Set(x => x.LastIdTransact, lastIdTransact);
            update.AddToSet("CountUpdate", countUpdate + 1);
            if (credit.HasValue)
            {
                update.AddToSet("Credit", credit.Value);
            }
            if (virtualDebit.HasValue)
            {
                update.AddToSet("VirtualDebit", virtualDebit.Value);
            }
            if (quanity.HasValue)
            {
                update.AddToSet("Quanity", quanity.Value);
            }

            var result = await _mongoDatabase.GetCollection<OrderValue>(_orderValueCollection)
                .FindOneAndUpdateAsync(filter, update);
            return result != null;
        }
    }
}
