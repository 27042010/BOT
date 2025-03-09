using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using MercadoPago.Client;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;

        private IStringLocalizer<PaymentService> _localizer;

        public PaymentService(
            IConfiguration configuration,
            ICacheService cacheService,
            IStringLocalizer<PaymentService> localizer)
        {
            _localizer = localizer;
            _configuration = configuration;
            _cacheService = cacheService;
        }

        public async Task<Payment> GeneratePix(string userId, int quantity)
        {
            try
            {
                MercadoPagoConfig.AccessToken = _configuration["MercadoPago:ApiKey"];

                var requestOptions = new RequestOptions();
                requestOptions.CustomHeaders.Add("x-idempotency-key", Guid.NewGuid().ToString());

                var request = new PaymentCreateRequest
                {
                    TransactionAmount = quantity,
                    Description = $"NinjaBot - {_localizer["recharge"]}",
                    PaymentMethodId = "pix",
                    Installments = 1,
                    IssuerId = userId,
                    DateOfExpiration = DateTime.Now.AddMinutes(15),
                    Payer = new PaymentPayerRequest
                    {
                        Email = _configuration["MercadoPago:Email"]
                    },
                };

                var client = new PaymentClient();
                Payment payment = await client.CreateAsync(request, requestOptions);

                SessionPaymentCache session = new()
                {
                    UserId = userId,
                    PaymentId = payment.Id,
                    Quantity = quantity
                };

                await _cacheService.SetMinutes($"{payment.Id}", JsonConvert.SerializeObject(session), 15);

                return payment;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Payment> GetPaymentById(long id)
        {
            MercadoPagoConfig.AccessToken = _configuration["MercadoPago:ApiKey"];

            var requestOptions = new RequestOptions();
            requestOptions.CustomHeaders.Add("x-idempotency-key", Guid.NewGuid().ToString());

            var client = new PaymentClient();
            Payment payment = await client.GetAsync(id);

            return payment;
        }
    }
}
