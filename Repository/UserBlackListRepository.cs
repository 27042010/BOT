using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserBlackListRepository : IUserBlackListRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserBlackList> _collection;

        public UserBlackListRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserBlackList>(UserBlackList.TABLE);
        }

        public async Task Record(UserBlackList userBlackList)
        {
            await _collection.InsertOneAsync(userBlackList);
        }

        public async Task DeleteByChatId(string chatId)
        {
            var filter = Builders<UserBlackList>.Filter.Eq(x => x.ChatId, chatId);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<UserBlackList> GetByChatId(string chatId)
        {
            var filter = Builders<UserBlackList>.Filter.Eq(x => x.ChatId, chatId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
