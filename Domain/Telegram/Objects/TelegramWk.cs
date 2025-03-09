using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Domain.Telegram.Objects
{
    public class TelegramWk
    {
        [JsonProperty("update_id")]
        public int? UpdateId { get; set; }

        [JsonProperty("message")]
        public Message? Message { get; set; }

        [JsonProperty("callback_query")]
        public CallbackQuery? CallbackQuery { get; set; }
    }

    public class Chat
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string? LastName { get; set; }

        [JsonProperty("username")]
        public string? Username { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }
    }

    public class From
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("is_bot")]
        public bool? IsBot { get; set; }

        [JsonProperty("first_name")]
        public string? FirstName { get; set; }

        [JsonProperty("last_name")]
        public string? LastName { get; set; }

        [JsonProperty("username")]
        public string? Username { get; set; }

        [JsonProperty("language_code")]
        public string? LanguageCode { get; set; }
    }

    public class Message
    {
        [JsonProperty("message_id")]
        public int? MessageId { get; set; }

        [JsonProperty("from")]
        public From From { get; set; }

        [JsonProperty("chat")]
        public Chat Chat { get; set; }

        [JsonProperty("date")]
        public long? Date { get; set; }

        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("video")]
        public Video? Video { get; set; }

        [JsonProperty("document")]
        public Document? Document { get; set; }

        [JsonProperty("sticker")]
        public Sticker? Sticker { get; set; }

        [JsonProperty("voice")]
        public Voice? Voice { get; set; }

        [JsonProperty("audio")]
        public Audio? Audio { get; set; }

        [JsonProperty("photo")]
        public List<Photo>? Photo { get; set; }

        [JsonProperty("reply_markup")]
        public ReplyMarkup? ReplyMarkup { get; set; }

        [JsonProperty("caption")]
        public string? Caption { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string? ReplyData { get; set; }
    }

    public class ReplyMarkup
    {
        [JsonProperty("inline_keyboard")]
        public List<List<InlineKeyboard>>? InlineKeyboard { get; set; }
    }

    public class InlineKeyboard
    {
        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("callback_data")]
        public string? CallbackData { get; set; }
    }

    public class Thumb
    {
        [JsonProperty("file_id")]
        public string? FileId { get; set; }

        [JsonProperty("file_unique_id")]
        public string? FileUniqueId { get; set; }

        [JsonProperty("file_size")]
        public int? FileSize { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }
    }

    public class Thumbnail
    {
        [JsonProperty("file_id")]
        public string? FileId { get; set; }

        [JsonProperty("file_unique_id")]
        public string? FileUniqueId { get; set; }

        [JsonProperty("file_size")]
        public int? FileSize { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }
    }

    public class Video
    {
        [JsonProperty("duration")]
        public int? Duration { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("file_name")]
        public string? FileName { get; set; }

        [JsonProperty("mime_type")]
        public string? MimeType { get; set; }

        [JsonProperty("thumbnail")]
        public Thumbnail? Thumbnail { get; set; }

        [JsonProperty("thumb")]
        public Thumb? Thumb { get; set; }

        [JsonProperty("file_id")]
        public string? FileId { get; set; }

        [JsonProperty("file_unique_id")]
        public string? FileUniqueId { get; set; }

        [JsonProperty("file_size")]
        public int? FileSize { get; set; }
    }

    public class Document
    {
        [JsonProperty("file_name")]
        public string? FileName { get; set; }

        [JsonProperty("mime_type")]
        public string? MimeType { get; set; }

        [JsonProperty("thumbnail")]
        public Thumbnail? Thumbnail { get; set; }

        [JsonProperty("thumb")]
        public Thumb? Thumb { get; set; }

        [JsonProperty("file_id")]
        public string? FileId { get; set; }

        [JsonProperty("file_unique_id")]
        public string? FileUniqueId { get; set; }

        [JsonProperty("file_size")]
        public int? FileSize { get; set; }
    }

    public class Voice
    {
        [JsonProperty("duration")]
        public int? Duration { get; set; }

        [JsonProperty("mime_type")]
        public string? MimeType { get; set; }

        [JsonProperty("file_id")]
        public string? FileId { get; set; }

        [JsonProperty("file_unique_id")]
        public string? FileUniqueId { get; set; }

        [JsonProperty("file_size")]
        public int? FileSize { get; set; }
    }

    public class Audio
    {
        [JsonProperty("duration")]
        public int? Duration { get; set; }

        [JsonProperty("file_name")]
        public string? FileName { get; set; }

        [JsonProperty("mime_type")]
        public string? MimeType { get; set; }

        [JsonProperty("file_id")]
        public string? FileId { get; set; }

        [JsonProperty("file_unique_id")]
        public string? FileUniqueId { get; set; }

        [JsonProperty("file_size")]
        public int? FileSize { get; set; }
    }

    public class Sticker
    {
        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }

        [JsonProperty("emoji")]
        public string? Emoji { get; set; }

        [JsonProperty("set_name")]
        public string? SetName { get; set; }

        [JsonProperty("is_animated")]
        public bool? IsAnimated { get; set; }

        [JsonProperty("is_video")]
        public bool? IsVideo { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("thumbnail")]
        public Thumbnail? Thumbnail { get; set; }

        [JsonProperty("thumb")]
        public Thumb? Thumb { get; set; }

        [JsonProperty("file_id")]
        public string? FileId { get; set; }

        [JsonProperty("file_unique_id")]
        public string? FileUniqueId { get; set; }

        [JsonProperty("file_size")]
        public int? FileSize { get; set; }
    }

    public class Photo
    {
        [JsonProperty("file_id")]
        public string? FileId { get; set; }

        [JsonProperty("file_unique_id")]
        public string? FileUniqueId { get; set; }

        [JsonProperty("file_size")]
        public long? FileSize { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }
    }

    public class CallbackQuery
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("from")]
        public From? From { get; set; }

        [JsonProperty("message")]
        public Message? Message { get; set; }

        [JsonProperty("chat_instance")]
        public string? ChatInstance { get; set; }

        [JsonProperty("data")]
        public string? Data { get; set; }
    }
}
