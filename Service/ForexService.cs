using Domain.Forex.Objects;
using Domain.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using Utils;

namespace Service
{
    public class ForexService : IForexService
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;

        public ForexService(IConfiguration configuration, 
            ICacheService cacheService)
        {
            _configuration = configuration;
            _cacheService = cacheService;
        }

        public async Task<double> GetForex(string from, string to)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            async Task<double> GetForexOnline()
            {
                var uriBuilder = new UriBuilder("https://www.alphavantage.co/query");

                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query["function"] = "FX_DAILY";
                query["from_symbol"] = from;
                query["to_symbol"] = to;
                query["apikey"] = _configuration["Forex:ApiKey"];
                uriBuilder.Query = query.ToString();

                using var client = new HttpClient();

                var response = await client.GetAsync(uriBuilder.ToString());
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var rs = JsonConvert.DeserializeObject<ResponseGetForex>(responseContent);

                    if(rs.TimeSeries != null)
                    {
                        double forex = rs.TimeSeries.Values.First().Open;
                        return forex;
                    }
                }
                
                return double.Parse(_configuration["Forex:Rublo"], NumberStyles.Number, culture);
            }

            double forex;

            var forexcache = await _cacheService.Get(Strings.FOREX);
            if (forexcache != null)
                forex = double.Parse($"{forexcache}", NumberStyles.Number, culture);
            else
                forex = await GetForexOnline();

            var s = forex.ToString().Replace(",", ".");
            await _cacheService.Set(Strings.FOREX, s, 24);

            return forex;
        }
    }
}
