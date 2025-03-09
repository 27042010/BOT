namespace Domain.Interfaces.Repository
{

    public interface IUserBalanceHistoryRepository
    {
        Task Record(UserBalanceHistory userBalanceHistory);
        Task Update(UserBalanceHistory userBalanceHistory);
        Task<UserBalanceHistory> GetBalanceHistoryByTransactionId(string chatId);
        Task<List<UserBalanceHistory>> GetBalanceHistoryByChatIdAndStatus(string chatId, string status, TransationType transationType);
        Task<List<UserBalanceHistory>> GetBalanceHistoryByChatIdAndStatus(string chatId, TransationType transationType);
        Task Delete(string guid);
    }
}
