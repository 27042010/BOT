using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<User> _collection;

        public UserRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<User>(User.TABLE);
        }

        public async Task Record(User user)
        {
            await _collection.InsertOneAsync(user);
        }

        public async Task Update(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _collection.ReplaceOneAsync(filter, user);
        }

        public async Task<User> GetByChatId(string chatId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.ChatId, chatId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
