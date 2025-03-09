using MercadoPago.Resource.Payment;

namespace Domain.Interfaces.Service
{
    public interface IPaymentService
    {
        Task<Payment> GeneratePix(string userId, int quantity);

        Task<Payment> GetPaymentById(long id);
    }
}
