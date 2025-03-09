using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class Functions
    {
        public static string ReplaceVariable(string str, string variableName, string replacement)
        {
            string pattern = "{{" + Regex.Escape(variableName) + "}}";
            string newStr = Regex.Replace(str, pattern, replacement);
            return newStr;
        }

        public static string EscapeMarkdownV2(string text, bool italic = false, bool bold = false, bool code = false, bool pre = false, bool strike = false, bool underline = false)
        {
            // Lista de caracteres especiais no MarkdownV2 do Telegram que precisam de escape
            string[] specialCharacters = { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };

            // Escapando caracteres especiais
            foreach (var specialChar in specialCharacters)
            {
                text = text.Replace(specialChar, "\\" + specialChar);
            }

            // Aplicando formatação Markdown conforme solicitado
            if (italic) text = $"_{text}_";
            if (bold) text = $"*{text}*";
            if (code) text = $"`{text}`";
            if (pre) text = $"```\n{text}\n```";
            if (strike) text = $"~{text}~";
            if (underline) text = $"<u>{text}</u>";

            return text;
        }


        public static string IsoCountryCodeToFlagEmoji(string country)
        {
            return string.Concat(country.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
        }

        public static string HashHMAC256(string text, string key)
        {
            // Converter a chave secreta e a mensagem em arrays de bytes
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(text);

            // Criar um objeto HMAC usando SHA256 como algoritmo de hash
            using HMACSHA256 hmac = new HMACSHA256(keyBytes);
            // Calcular o HMAC para a mensagem
            byte[] hmacBytes = hmac.ComputeHash(messageBytes);

            // Converter o resultado para uma string hexadecimal
            string hmacString = BitConverter.ToString(hmacBytes).Replace("-", "").ToLower();

            return hmacString;
        }

        public static CultureInfo GetCultureByEnglishName(String englishName)
        {
            // create an array of CultureInfo to hold all the cultures found, 
            // these include the users local culture, and all the
            // cultures installed with the .Net Framework
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
            // get culture by it's english name 
            var culture = cultures.FirstOrDefault(c =>
                c.EnglishName.Equals(englishName, StringComparison.InvariantCultureIgnoreCase));
            return culture;
        }
    }
}
