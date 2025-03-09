using Domain;

namespace Service
{
    public interface IUserService
    {
        Task Record(User user);
        Task Update(User user);
        Task<User> GetUserByChat(string chatId);

        Task<UserBalance> GetUserBalanceByChat(string chatId);
        Task RecordBalance(UserBalance history);

        Task<string> RecordBalanceHistory(UserBalanceHistory history);
        Task<UserBalanceHistory> GetBalanceHistoryByTransactionId(string transationId);
        Task<List<UserBalanceHistory>> GetBalanceHistoryByChatIdAndStatus(string chatId, string status, TransationType transationType);
        Task<List<UserBalanceHistory>> GetBalanceHistoryByChatIdAndStatus(string chatId, TransationType transationType);
        Task DeleteBalanceHistoryByGuid(string guid);

        Task BanUser(string chatId, BlackListReason reason);
        Task UnBanUser(string chatId);
        Task<UserBlackList> GetBan(string chatId);
    }
}
