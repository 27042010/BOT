namespace Domain.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task Record(User user);
        Task Update(User user);
        Task<User> GetByChatId(string chatId);
    }
}
