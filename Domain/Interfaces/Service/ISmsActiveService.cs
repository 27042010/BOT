using Domain.ActiveSms;
using Domain.ActiveSms.Objects;
using Newtonsoft.Json.Linq;

namespace Domain.Interfaces.Service
{
    public interface ISmsActiveService
    {
        Task<ActiveSmsServices> GetServicesAndPrices(string countryDisplayName);

        Task<ResponseRequestAtivationService> RequestAtivationService(string service, string country, string maxPrice);

        Task<bool> UpdateStatusAtivationService(string activationId, StatusActivation status);

        Task<string> RePurchaseNumber(string activationId);

        Task<JObject> GetCurrentPriceByService(string service, string countryDisplayName);
    }
}
