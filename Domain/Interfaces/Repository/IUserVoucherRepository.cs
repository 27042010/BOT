namespace Domain.Interfaces.Repository
{
    public interface IUserVoucherRepository
    {
        Task Record(UserVoucher userVoucher);
        Task<UserVoucher> GetByCodeAndChatId(string code, string chatId);
    }
}
