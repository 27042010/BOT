using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Voucher> _collection;

        public VoucherRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<Voucher>(Voucher.TABLE);
        }

        public async Task Record(Voucher voucher) => await _collection.InsertOneAsync(voucher);

        public async Task<Voucher> GetByCode(string code)
        {
            var filter = Builders<Voucher>.Filter.Eq(x => x.Code, code);
            var sort = Builders<Voucher>.Sort.Descending(x => x.CreatedAt);
            return await _collection.Find(filter).Sort(sort).FirstOrDefaultAsync();
        }

        public async Task Update(Voucher voucher)
        {
            var filter = Builders<Voucher>.Filter.Eq(x => x.Id, voucher.Id);
            await _collection.ReplaceOneAsync(filter, voucher);
        }

        public async Task<Voucher> GetByDate(DateTime date)
        {
            var filter = Builders<Voucher>.Filter.And(
                Builders<Voucher>.Filter.Gte(x => x.CreatedAt, date.Date),
                Builders<Voucher>.Filter.Lte(x => x.CreatedAt, date.Date.AddDays(1).AddTicks(-1)));

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
