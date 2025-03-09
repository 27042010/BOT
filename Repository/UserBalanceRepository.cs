using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserBalanceRepository : IUserBalanceRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserBalance> _collection;

        public UserBalanceRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserBalance>(UserBalance.TABLE);
        }

        public async Task Record(UserBalance userBalance)
        {
            await _collection.InsertOneAsync(userBalance);
        }

        public async Task Update(UserBalance userBalance)
        {
            var filter = Builders<UserBalance>.Filter.Eq(ub => ub.ChatId, userBalance.ChatId);
            await _collection.ReplaceOneAsync(filter, userBalance);
        }

        public async Task<UserBalance> GetByChatId(string chatId)
        {
            var filter = Builders<UserBalance>.Filter.Eq(ub => ub.ChatId, chatId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
