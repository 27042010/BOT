using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;
using System;

namespace Repository
{
    public class UserBalanceHistoryRepository : IUserBalanceHistoryRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserBalanceHistory> _collection;

        public UserBalanceHistoryRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserBalanceHistory>(UserBalanceHistory.TABLE);
        }

        public async Task Delete(string guid)
        {
            var filter = Builders<UserBalanceHistory>.Filter.Eq(x => x.Guid, guid);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<List<UserBalanceHistory>> GetBalanceHistoryByChatIdAndStatus(string chatId, string status, TransationType transationType)
        {
            var filter = Builders<UserBalanceHistory>.Filter.And(
                Builders<UserBalanceHistory>.Filter.Eq(x => x.ChatId, chatId),
                Builders<UserBalanceHistory>.Filter.Eq(x => x.Status, status),
                Builders<UserBalanceHistory>.Filter.Eq(x => x.TransationType, transationType));

            var sort = Builders<UserBalanceHistory>.Sort.Descending(x => x.CreatedAt);

            return await _collection.Find(filter).Sort(sort).ToListAsync();
        }

        public async Task<List<UserBalanceHistory>> GetBalanceHistoryByChatIdAndStatus(string chatId, TransationType transationType)
        {
            var filter = Builders<UserBalanceHistory>.Filter.And(
                Builders<UserBalanceHistory>.Filter.Eq(x => x.ChatId, chatId),
                Builders<UserBalanceHistory>.Filter.Eq(x => x.TransationType, transationType));

            var sort = Builders<UserBalanceHistory>.Sort.Descending(x => x.CreatedAt);

            return await _collection.Find(filter).Sort(sort).ToListAsync();
        }

        public async Task<UserBalanceHistory> GetBalanceHistoryByTransactionId(string transactionId) => await _collection.Find(x => x.TransactionId == transactionId).FirstOrDefaultAsync();

        public async Task Record(UserBalanceHistory userBalanceHistory)
        {
            await _collection.InsertOneAsync(userBalanceHistory);
        }

        public async Task Update(UserBalanceHistory userBalanceHistory)
        {
            var filter = Builders<UserBalanceHistory>.Filter.Eq(ub => ub.Guid, userBalanceHistory.Guid);
            await _collection.ReplaceOneAsync(filter, userBalanceHistory);
        }
    }
}
