using Domain.ActiveSms.Objects;
using Domain.Telegram.Objects;
using Microsoft.Extensions.Localization;

namespace Domain.Interfaces.Service
{
    public interface IBotSmsService
    {
        Task OnReceiveMessage(TelegramWk webhookData);
        Task ConfirmPix(long paymentId);
        Task OnSmsActiveReceiveSms(WKSms wkSms);
        Task GenerateVoucher(IStringLocalizer<object> localizer);
        Task<Voucher> VoucherToday(); // Pega o voucher do dia se existir
    }
}
