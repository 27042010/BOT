using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserVoucherRepository : IUserVoucherRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserVoucher> _collection;

        public UserVoucherRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserVoucher>(UserVoucher.TABLE);
        }

        public async Task Record(UserVoucher userVoucher)
        {
            await _collection.InsertOneAsync(userVoucher);
        }

        public async Task<UserVoucher> GetByCodeAndChatId(string code, string chatId)
        {
            var filter = Builders<UserVoucher>.Filter.And(
                Builders<UserVoucher>.Filter.Eq(x => x.Code, code),
                Builders<UserVoucher>.Filter.Eq(x => x.ChatId, code));

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
