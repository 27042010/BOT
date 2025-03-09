using Domain._5Sim;

namespace Domain.Interfaces.Service
{
    public interface I5SimService
    {
        Task<Dictionary<string, FiveSimServices>> GetServicesAndPrices(string countryDisplayName);

        Task<ResponseActivateRequest> RequestAtivationService(string countryDisplayName, string product, string maxPrice);

        Task<bool> CancelOrder(string orderId);

        Task<ResponseActivateRequest> CheckOrder(string orderId);

        Task<ResponseActivateRequest> RePurchaseNumber(string product, string phoneNumber);
    }
}
