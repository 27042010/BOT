using API.Payload;
using Domain.Interfaces.Service;
using Domain.Telegram.Objects;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("telegram")]
    public class TelegramController : Controller
    {
        private IConfiguration _configuration;
        private ITelegramService _telegramService;
        private IBotSmsService _botSmsService;

        public TelegramController(
            ITelegramService telegramService,
            IConfiguration configuration,
            IBotSmsService botSmsService)
        {
            _telegramService = telegramService;
            _configuration = configuration;
            _botSmsService = botSmsService;

        }

        [HttpPost("set/webhook")]
        public async Task<IActionResult> SetWebhook([FromBody] SetWebhookTelegram payload)
        {
            try
            {
                if (payload == null) return Unauthorized();
                if (payload.Secret != _configuration["Secret"]) return BadRequest();

                await _telegramService.SetWebhook(payload.Url);

                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
