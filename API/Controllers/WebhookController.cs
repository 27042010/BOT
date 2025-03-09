using API.Payload;
using Domain.ActiveSms.Objects;
using Domain.Interfaces.Service;
using Domain.Telegram.Objects;
using MercadoPago.Resource.Payment;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utils;

namespace API.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private IConfiguration _configuration;
        private ITelegramService _telegramService;
        private IBotSmsService _botSmsService;

        public WebhookController(IConfiguration configuration,
            ITelegramService telegramService,
            IBotSmsService botSmsService)
        {
            _configuration = configuration;
            _telegramService = telegramService;
            _botSmsService = botSmsService;
        }

        [HttpPost("telegram")]
        public async Task<IActionResult> Telegram([FromBody] TelegramWk payload)
        {
            try
            {
                string? securityToken = HttpContext.Request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault()?.Split(" ").Last();

                if (securityToken == null) return Unauthorized();
                if (securityToken != _configuration["Telegram:SecurityToken"]) return BadRequest();

                await _botSmsService.OnReceiveMessage(payload);

                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("mercadopago")]
        public async Task<IActionResult> MercadoPago([FromBody] MercadoPagoWK payload)
        {
            try
            {
                if (payload == null) return BadRequest();

                string? xsignature = HttpContext.Request.Headers["x-signature"].FirstOrDefault();
                string? xrequestId = HttpContext.Request.Headers["x-request-id"].FirstOrDefault().Split(" ").Last();

                string ts = xsignature.Split(",").ToList().First().Split("=").ToList().Last();
                string v1 = xsignature.Split(",").ToList().Last().Split("=").ToList().Last();

                var payment = JsonConvert.DeserializeObject<Payment>(JsonConvert.SerializeObject(payload.Data));

                string hash = $"id:{payment.Id};request-id:{xrequestId};ts:{ts};";
                string secret = Functions.HashHMAC256(hash, _configuration["MercadoPago:XSignature"]);

                if (v1 != secret)
                    return Unauthorized();

                if(payload.Action == "payment.updated")
                {
                    await _botSmsService.ConfirmPix((long)payment.Id);
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("sms/activesms")]
        public async Task<IActionResult> ActiveSms([FromBody] WKSms payload)
        {
            try
            {
                await _botSmsService.OnSmsActiveReceiveSms(payload);
                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

    }
}
