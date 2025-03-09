using Telegram.Bot.Types.ReplyMarkups;

namespace Domain.Models
{
    public class PaginationInlineOptionsKeyboards
    {
        public List<InlineKeyboardButton[][]> Groups { get; set; } = new();
        public int TotalPages { get; set; }
    }
}
