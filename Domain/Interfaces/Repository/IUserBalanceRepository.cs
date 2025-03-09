namespace Domain.Interfaces.Repository
{
    public interface IUserBalanceRepository
    {
        Task Record(UserBalance userBalance);
        Task Update(UserBalance userBalance);
        Task<UserBalance> GetByChatId(string chatId);
    }
}
