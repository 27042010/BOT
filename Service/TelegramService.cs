using Domain.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Service
{
    public class TelegramService : ITelegramService
    {
        private readonly IConfiguration _configuration;
        private readonly TelegramBotClient _client;

        public TelegramService(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new TelegramBotClient(_configuration["Telegram:BotToken"]);
        }

        public async Task AnswerCallbackQueryAsync(string messageId) => await _client.AnswerCallbackQueryAsync(messageId);

        public async Task DeleteMessage(string chatId, int messageId)
        {
            await _client.DeleteMessageAsync(chatId, messageId);
        }

        public async Task EditCaption(string chatId, int messageId, string newCaption, InlineKeyboardMarkup inlineKeyboardMarkup)
        {
            await _client.EditMessageCaptionAsync(chatId: chatId, messageId: messageId, caption: newCaption, replyMarkup: inlineKeyboardMarkup, parseMode: ParseMode.MarkdownV2);
        }

        public async Task EditReplyMarkup(string chatId, int messageId, InlineKeyboardMarkup inlineKeyboardMarkup)
        {
            try
            {
                await _client.EditMessageReplyMarkupAsync(chatId: chatId, messageId: messageId, replyMarkup: inlineKeyboardMarkup);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task GetChannel(string chatId)
        {
            var chat = await _client.GetChatAsync(chatId);
        }

        public async Task<int> SendMedia(string chatId, string type, string url, string caption, InlineKeyboardMarkup? inlineKeyboardMarkup)
        {
            int messageId = 0;

            try
            {
                if (type == "photo")
                {
                    var message = await _client.SendPhotoAsync(
                        chatId: chatId,
                        photo: InputFile.FromUri(url),
                        caption: caption,
                        parseMode: ParseMode.MarkdownV2,
                        replyMarkup: inlineKeyboardMarkup);

                    messageId = message.MessageId;

                }
                else if (type == "sticker")
                {
                    var message = await _client.SendStickerAsync(
                        chatId: chatId,
                        sticker: InputFile.FromUri(url));

                    messageId = message.MessageId;
                }

                return messageId;
            }
            catch (Exception ex)
            {
                throw new Exception(JsonConvert.SerializeObject(ex));
            }
        }

        public async Task SendReplyMarkup(string chatId, string text, InlineKeyboardMarkup inlineKeyboardMarkup)
        {
            try
            {
                await _client.SendTextMessageAsync(
                                chatId: chatId,
                                text: text,
                                replyMarkup: inlineKeyboardMarkup);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SendText(string chatId, string text)
        {
            try
            {
                await _client.SendTextMessageAsync(
                                chatId: chatId,
                                text: text,
                                parseMode: ParseMode.MarkdownV2);

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task SetWebhook(string url)
        {
            try
            {
                var tokenDoBot = _configuration["Telegram:SecurityToken"];
                await _client.SetWebhookAsync(url: url, secretToken: tokenDoBot);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
    }
}
