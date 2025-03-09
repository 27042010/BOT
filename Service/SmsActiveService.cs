using Domain.ActiveSms;
using Domain.ActiveSms.Objects;
using Domain.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utils;

namespace Service
{
    public class SmsActiveService : ISmsActiveService
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;

        public SmsActiveService(IConfiguration configuration, 
            ICacheService cacheService)
        {
            _configuration = configuration;
            _cacheService = cacheService;
        }

        public async Task<ActiveSmsServices> GetServicesAndPrices(string countryDisplayName)
        {
            async Task<ActiveSmsServices> GetServices()
            {
                var uriBuilder = new UriBuilder("https://api.sms-activate.org/stubs/handler_api.php");

                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query["api_key"] = _configuration["SmsActive:ApiKey"];
                query["action"] = "getRentServicesAndCountries";
                query["country"] = GetCountryCode(countryDisplayName);
                uriBuilder.Query = query.ToString();

                using var client = new HttpClient();

                var response = await client.GetAsync(uriBuilder.ToString());
                var responseContent = await response.Content.ReadAsStringAsync();

                await _cacheService.Set(Strings.SERVICES_ACTIVESMS, responseContent, 1);

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<ActiveSmsServices>(responseContent);
                else
                    throw new Exception(responseContent);
            }

            return await GetServices();
        }

        public async Task<ResponseRequestAtivationService> RequestAtivationService(string service, string countryDisplayName, string maxPrice)
        {
            var uriBuilder = new UriBuilder("https://sms-activate.org/stubs/handler_api.php");

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["api_key"] = _configuration["SmsActive:ApiKey"];
            query["action"] = "getNumberV2";
            query["service"] = service;
            query["country"] = GetCountryCode(countryDisplayName);
            //query["maxPrice"] = double.Parse(maxPrice) > 120 ? "120" : maxPrice;
            uriBuilder.Query = query.ToString();

            using var client = new HttpClient();

            var response = await client.GetAsync(uriBuilder.ToString());
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && responseContent != "NO_NUMBERS" && responseContent != "NO_BALANCE")
            {
                var rs = JsonConvert.DeserializeObject<ResponseRequestAtivationService?>(responseContent);

                if (rs == null)
                    return null;

                return rs;
            }
            else
                return null;
        }

        public async Task<bool> UpdateStatusAtivationService(string activationId, StatusActivation status)
        {
            var currentStatus = await GetStatusActivation(activationId);

            if (currentStatus == "STATUS_CANCEL" || currentStatus == "STATUS_OK") return true;

            var uriBuilder = new UriBuilder("https://sms-activate.org/stubs/handler_api.php");

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["api_key"] = _configuration["SmsActive:ApiKey"];
            query["id"] = activationId;
            query["action"] = "setStatus";
            query["status"] = $"{(int)status}";
            uriBuilder.Query = query.ToString();

            using var client = new HttpClient();

            var response = await client.GetAsync(uriBuilder.ToString());
            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent == "ACCESS_CANCEL";
        }

        protected async Task<string> GetStatusActivation(string activationId)
        {
            var uriBuilder = new UriBuilder("https://sms-activate.org/stubs/handler_api.php");

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["api_key"] = _configuration["SmsActive:ApiKey"];
            query["id"] = activationId;
            query["action"] = "getStatus";
            uriBuilder.Query = query.ToString();

            using var client = new HttpClient();

            var response = await client.GetAsync(uriBuilder.ToString());
            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }

        protected string GetCountryCode(string countryDisplayName)
        {
            var country = Strings.Countries.Where(c => c.Title == countryDisplayName).FirstOrDefault();

            string countryCode = "73";
            if (country != null)
                countryCode = country.CountryNumber;

            return countryCode;
        }

        public async Task<string?> RePurchaseNumber(string activationId)
        {
            var uriBuilder = new UriBuilder("https://sms-activate.org/stubs/handler_api.php");

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["api_key"] = _configuration["SmsActive:ApiKey"];
            query["action"] = "getExtraActivation";
            query["activationId"] = activationId;
            uriBuilder.Query = query.ToString();

            using var client = new HttpClient();

            var response = await client.GetAsync(uriBuilder.ToString());
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode 
                && responseContent != "RENEW_ACTIVATION_NOT_AVAILABLE" 
                && responseContent != "NO_BALANCE"
                && responseContent != "NEW_ACTIVATION_IMPOSSIBLE"
                && responseContent != "ERROR_SQL"
                && responseContent.Split(":").Length == 3)
            {
                return responseContent;
            }
            else
                return null;
        }

        public async Task<JObject> GetCurrentPriceByService(string service, string countryDisplayName)
        {
            var uriBuilder = new UriBuilder("https://api.sms-activate.org/stubs/handler_api.php");

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["api_key"] = _configuration["SmsActive:ApiKey"];
            query["service"] = service;
            query["action"] = "getPrices";
            query["country"] = GetCountryCode(countryDisplayName);
            uriBuilder.Query = query.ToString();

            using var client = new HttpClient();

            var response = await client.GetAsync(uriBuilder.ToString());
            var responseContent = await response.Content.ReadAsStringAsync();

            await _cacheService.Set(Strings.SERVICES_ACTIVESMS, responseContent, 1);

            if (response.IsSuccessStatusCode)
                return JObject.Parse(responseContent);
            else
                throw new Exception(responseContent);
        }
    }
}
