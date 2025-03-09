namespace Domain.Interfaces.Repository
{
    public interface IUserBlackListRepository
    {
        Task Record(UserBlackList userBlackList);
        Task DeleteByChatId(string chatId);
        Task<UserBlackList> GetByChatId(string chatId);
    }
}
