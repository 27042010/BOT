using Telegram.Bot.Types.ReplyMarkups;

namespace Domain.Interfaces.Service
{
    public interface ITelegramService
    {
        Task SetWebhook(string url);

        Task SendText(string chatId, string text);

        Task SendReplyMarkup(string chatId, string text, InlineKeyboardMarkup inlineKeyboardMarkup);

        Task EditReplyMarkup(string chatId, int messageId, InlineKeyboardMarkup inlineKeyboardMarkup);

        Task<int> SendMedia(string chatId, string type, string url, string caption, InlineKeyboardMarkup? inlineKeyboardMarkup);

        Task EditCaption(string chatId, int messageId, string newCaption, InlineKeyboardMarkup inlineKeyboardMarkup);

        Task AnswerCallbackQueryAsync(string messageId);

        Task DeleteMessage(string chatId, int messageId);

        Task GetChannel(string chatId);
    }
}
