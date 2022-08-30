using eventList.CustomTypes;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserBalanceMicroservice.Configs;
using UserBalanceMicroservice.Models;
namespace UserBalanceMicroservice
{
    public class BalanceContext
    {
        private readonly IMongoDatabase _mongoDatabase;

        private const string _userCollection = "User";
        private const string _transactCollection = "Transact";
        public BalanceContext(
            IOptions<DatabaseSettings> balanceDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                balanceDatabaseSettings.Value.ConnectionString);

            _mongoDatabase = mongoClient.GetDatabase(
                balanceDatabaseSettings.Value.DatabaseName);
        }

        public async Task<bool> ChangeBalanceAsync(string idUser, decimal sum, bool isAdd, string transactionId)
        {
            var user = await GetOrCreateUserAsync(idUser);
            string lastTransactId = user.LastTransactId;
            var tempMoney = user.Money;

            if(user.LastTransactId == transactionId) { return false; }

            var filter = Builders<User>.Filter.Where(x => x.Id == idUser
                    && x.LastTransactId == lastTransactId
                    && x.Money == tempMoney);

            if (isAdd)
            {
                user.Money += sum;
            }
            else
            {
                if (user.Money < sum)
                {
                    return false;
                }
                user.Money -= sum;
            }

            var update = Builders<User>.Update.Set(x => x.Money, user.Money).Set(x => x.LastTransactId, transactionId);

            var temp = await _mongoDatabase.GetCollection<User>(_userCollection)
                    .FindOneAndUpdateAsync(filter, update);
            if (temp != null)
            {
                await SetTransactAsync(idUser, isAdd, transactionId);
                return true;
            }
            return false;
        }

        public async Task SetTransactAsync(string idUser, bool isAdd, string transactionId)
        {
            var transaction = await _mongoDatabase.GetCollection<Transact>(_transactCollection)
                .Find(x => x.Id == transactionId)
                .FirstOrDefaultAsync();

            if (transaction != null)
            {
                return;
            }

            var transact = new Transact()
            {
                GlobalTransactId = transactionId,
                Mode = isAdd ? EnumList.CustomTypes.Operation.Addition : EnumList.CustomTypes.Operation.Subtract,
                UserId = idUser
            };

            await _mongoDatabase.GetCollection<Transact>(_transactCollection)
                        .InsertOneAsync(transact);    
        }

        public async Task<User> GetBalanceAsync(string id)
        {
            return await GetOrCreateUserAsync(id);
        }

        private async Task<User> CreateUserAsync(string id)
        {
            await _mongoDatabase.GetCollection<User>(_userCollection)
                    .InsertOneAsync(new User() { Id = id });

            return await _mongoDatabase.GetCollection<User>(_userCollection)
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        private async Task<User> GetOrCreateUserAsync(string id)
        {
            var user = await _mongoDatabase.GetCollection<User>(_userCollection)
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();

            return user == null ? await CreateUserAsync(id) : user;
        }
    }
}
