namespace Domain.Interfaces.Repository
{
    public interface IVoucherRepository
    {
        Task Record(Voucher voucher);
        Task<Voucher> GetByCode(string code);
        Task<Voucher> GetByDate(DateTime date);
        Task Update(Voucher voucher);
    }
}
