using Domain._5Sim;
using Domain.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Service
{
    public class FiveSimService : I5SimService
    {
        private readonly IConfiguration _configuration;

        public FiveSimService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> CancelOrder(string orderId)
        {
            var uriBuilder = new UriBuilder($"https://5sim.net/v1/user/cancel/{orderId}");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["5Sim:ApiToken"]);

            var response = await client.GetAsync(uriBuilder.ToString());
            var responseContent = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode;
        }

        public async Task<ResponseActivateRequest> CheckOrder(string orderId)
        {
            var uriBuilder = new UriBuilder($"https://5sim.net/v1/user/check/{orderId}");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["5Sim:ApiToken"]);

            var response = await client.GetAsync(uriBuilder.ToString());
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ResponseActivateRequest>(responseContent);

            return null;
        }

        public async Task<Dictionary<string, FiveSimServices>> GetServicesAndPrices(string countryDisplayName)
        {
            try
            {
                string country = countryDisplayName.ToLower().Replace(" ", "");
                var uriBuilder = new UriBuilder($"https://5sim.net/v1/guest/products/{country}/any");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["5Sim:ApiToken"]);

                var response = await client.GetAsync(uriBuilder.ToString());
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<Dictionary<string, FiveSimServices>>(responseContent);

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<ResponseActivateRequest> RePurchaseNumber(string product, string phoneNumber)
        {
            try
            {
                var uriBuilder = new UriBuilder($"https://5sim.net/v1/user/reuse/{product.ToLower()}/{phoneNumber}");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["5Sim:ApiToken"]);

                var response = await client.GetAsync(uriBuilder.ToString());
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (responseContent != "no free phones" 
                        && responseContent != "reuse not possible"
                        && responseContent != "reuse false"
                        && responseContent != "reuse expired")
                    {
                        return JsonConvert.DeserializeObject<ResponseActivateRequest>(responseContent);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResponseActivateRequest> RequestAtivationService(string countryDisplayName, string product, string maxPrice)
        {
            try
            {
                string country = countryDisplayName.ToLower().Replace(" ", "");

                var uriBuilder = new UriBuilder($"https://5sim.net/v1/user/buy/activation/{country}/any/{product}?reuse");
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query["maxPrice"] = maxPrice;
                query["reuse"] = "1";
                uriBuilder.Query = query.ToString();

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["5Sim:ApiToken"]);

                var response = await client.GetAsync(uriBuilder.ToString());
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if(responseContent != "no free phones")
                    {
                        return JsonConvert.DeserializeObject<ResponseActivateRequest>(responseContent);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
