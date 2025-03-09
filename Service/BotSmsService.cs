using Domain;
using Domain.ActiveSms.Objects;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using Domain.Telegram.Objects;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;
using Utils;

namespace Service
{
    public class BotSmsService : IBotSmsService
    {
        private readonly IUserService _userService;
        private readonly ITelegramService _telegramService;
        private readonly ISmsActiveService _activeSmsService;
        private readonly I5SimService _fivesimService;
        private readonly IForexService _forexService;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly IPaymentService _paymentService;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IUserVoucherRepository _userVoucherRepository;

        private IStringLocalizer<BotSmsService> _localizer;

        private InlineKeyboardButton optionStart;
        private InlineKeyboardButton optionServices;
        private InlineKeyboardButton optionAddBalance;
        private InlineKeyboardButton optionAddVoucher;
        private InlineKeyboardButton optionMyProfile;
        private InlineKeyboardButton optionSupport;
        private InlineKeyboardButton optionMyPolices;
        private InlineKeyboardButton optionMyGroups;
        private InlineKeyboardButton optionsBackButton;
        private InlineKeyboardButton optionsNextButton;
        private InlineKeyboardButton optionsFirstButton;
        private InlineKeyboardButton optionsLastButton;
        private InlineKeyboardButton optionsConfirmRent;
        private InlineKeyboardButton optionsListOldServicesRented;
        private InlineKeyboardButton optionsListOldNumberRented;

        private InlineKeyboardButton optionServerSmsActive;
        private InlineKeyboardButton optionServer5Sim;

        Dictionary<string, string> servicesNamesForRenames;
        List<string> servicesToHide;

        private string _chatId = string.Empty;
        private string _firstName = string.Empty;
        private string _username = string.Empty;
        private string _userBalance = "";
        private double _userBalanceDouble = 0.0;
        private string _selectedCountry = string.Empty;
        private string _languageCode = string.Empty;

        private int _messageId = 0;
        private string _callbackMessageId = string.Empty;

        public BotSmsService(
            IUserService userService,
            ITelegramService telegramService,
            IConfiguration configuration,
            IStringLocalizer<BotSmsService> localizer,
            ISmsActiveService activeSmsService,
            I5SimService i5SimService,
            IForexService forexService,
            ICacheService cacheService,
            IPaymentService paymentService,
            IVoucherRepository voucherRepository,
            IUserVoucherRepository userVoucherRepository)
        {
            _voucherRepository = voucherRepository;
            _userVoucherRepository = userVoucherRepository;

            _userService = userService;
            _telegramService = telegramService;
            _configuration = configuration;
            _localizer = localizer;
            _activeSmsService = activeSmsService;
            _fivesimService = i5SimService;
            _forexService = forexService;
            _cacheService = cacheService;
            _paymentService = paymentService;

            MountBtns();
        }

        protected void MountBtns()
        {
            optionServerSmsActive = InlineKeyboardButton.WithCallbackData(text: $"📡 {_localizer["Server"]} {1}", callbackData: "/activesms");
            optionServer5Sim = InlineKeyboardButton.WithCallbackData(text: $"📡 {_localizer["Server"]} {2}", callbackData: "/5sim");

            optionStart = InlineKeyboardButton.WithCallbackData(text: $"\U0001f977 {_localizer["Home"]}", callbackData: "/start");
            optionServices = InlineKeyboardButton.WithCallbackData(text: $"💬 {_localizer["Receive SMS"]}", callbackData: "/services");
            optionsListOldServicesRented = InlineKeyboardButton.WithCallbackData(text: $"⚡️ {_localizer["Repurchase SMS"]}", callbackData: "/listOldRentedServicesSMS");
            optionsListOldNumberRented = InlineKeyboardButton.WithCallbackData(text: $"♻️ {_localizer["Repurchase Number"]}", callbackData: "/listOldRentedSMS");
            optionAddBalance = InlineKeyboardButton.WithCallbackData(text: $"💵 {_localizer["Add balance"]}", callbackData: "/add_balance");
            optionAddVoucher = InlineKeyboardButton.WithCallbackData(text: $"🔖 {_localizer["Add Voucher"]}", callbackData: "/add_voucher");
            optionMyProfile = InlineKeyboardButton.WithCallbackData(text: $"👤 {_localizer["My profile"]}", callbackData: "/my_profile");

            optionSupport = InlineKeyboardButton.WithUrl(text: $"🛠 {_localizer["Support"]}", url: _configuration["Telegram:SupportUrl"]);
            optionMyPolices = InlineKeyboardButton.WithWebApp(text: $"⚠️ {_localizer["Terms"]}", webAppInfo: new Telegram.Bot.Types.WebAppInfo() { Url = _configuration["Telegram:PolicyTermsUrl"] });
            optionMyGroups = InlineKeyboardButton.WithUrl(text: $"📢 {_localizer["Channel"]}", url: _configuration["Telegram:Group"]);
            optionsConfirmRent = InlineKeyboardButton.WithCallbackData($"✅ {_localizer["Confirm"]}", callbackData: "/confirmrent");

            optionsBackButton = InlineKeyboardButton.WithCallbackData($"◀️ {_localizer["previous page"]}", "/previous");
            optionsNextButton = InlineKeyboardButton.WithCallbackData($"{_localizer["next page"]} ▶️", "/next");
            optionsFirstButton = InlineKeyboardButton.WithCallbackData($"↩️ {_localizer["first page"]}", "/first");
            optionsLastButton = InlineKeyboardButton.WithCallbackData($"{_localizer["last page"]} ↪️", "/last");

            servicesNamesForRenames = new Dictionary<string, string>()
            {
                {"lf", "TikTok"},
                {"yw", "Grindrr"},
                {"dh", "eBay"},
                {"tg", "Telegram"},
                {"wb", "WeChat"},
                //{"ot", "Outros"},
                {"oy", "Tinder"},
                {"fq", "Outline"},
                {"yq", "mail.com"},
                {"xx", "Joyride"},
                {"ai", "Amazon"},
                {"am", "Amazon"},
                {"vt", "Innomax"},
                {"ok", "ok.ru"},
                {"sm", "YoWin"},
                {"fd", "MeetMe"},
                {"hw", "Alipay/Alibaba"},
                {"uk", "Airbn"},
                {"bl", "BIGO LIVE"},
                {"ua", "BlaBlaCar"},
                {"li", "Baidu"},
                {"bz", "Blizzard"},
                {"ip", "Burger King"},
                {"re", "Coinbase"},
                {"zk", "Deliveroo"},
                {"xk", "DiDi"},
                {"ds", "Discord"},
                {"sv", "Dostavista"},
                {"fb", "Facebook"},
                {"nz", "Foodpanda"},
                {"go", "Youtube/Gmail"},
                {"ig", "Instagram"},
                {"il", "IQOS"},
                {"kt", "KakaoTalk"},
                {"me", "Line messenger"},
                {"iq", "icq"},
                {"et", "Clubhouse"},
                {"gj", "Carousell"},
                {"pm", "AOL"},
                {"wa", "Whatsapp"},
                {"wx", "Apple"},
                {"full", "Qualquer"},
                {"ts", "PayPal"},
                {"mm", "Microsoft"},
                {"oi", "Tinder"},
                {"pf", "pof.com"},
                {"gm", "Mocospace"},
                {"fx", "PGbonus"},
                {"sn", "OLX"},
                {"zh", "Zoho"},
                {"nf", "Netflix"},
                {"fu", "Snapchat"},
                {"qq", "Tencent QQ"},
                {"tw", "Twitter"},
                {"mb", "Yahoo"},
                {"ub", "Uber"},
                {"nv", "Navery"},
                {"ka", "Shopee"},
                {"mt", "Steam"},
                {"ew", "Nike"},
                {"vi", "Viber"},
                {"tn", "LinkedIN"},
                {"dp", "ProtonMail"},
                {"dr", "OpenAI"},
            };

            servicesToHide = new List<string>()
            {
                "aoh",
                "kt",
                "fs",
                "vk",
                "yl",
                "ya",
                "ft",
                "kl",
                "ot",
            };
        }

        public async Task OnReceiveMessage(TelegramWk webhookData)
        {
            if (webhookData == null) return;

            string input;
            if (webhookData.Message != null && webhookData.Message.Text != null && webhookData.Message.From != null)
            {
                input = webhookData.Message.Text;
                _chatId = webhookData.Message.From.Id.ToString();
                _firstName = $"{webhookData.Message.From.FirstName} {webhookData.Message.From.LastName}";
                _username = webhookData.Message.Chat.Username ?? string.Empty;
                _languageCode = webhookData.Message.From.LanguageCode ?? string.Empty;
                _messageId = (int)webhookData.Message.MessageId;
            }
            else if (webhookData.CallbackQuery != null && webhookData.CallbackQuery.Data != null && webhookData.CallbackQuery.From != null)
            {
                _callbackMessageId = webhookData.CallbackQuery.Id;
                _messageId = (int)webhookData.CallbackQuery.Message.MessageId;

                input = webhookData.CallbackQuery.Data;
                _chatId = webhookData.CallbackQuery.From.Id.ToString();
                _firstName = $"{webhookData.CallbackQuery.From.FirstName} {webhookData.CallbackQuery.From.LastName}";
                _username = webhookData.CallbackQuery.From.Username ?? string.Empty;
                _languageCode = webhookData.CallbackQuery.From.LanguageCode ?? string.Empty;
            }
            else
                return;

            var userblacklist = await _userService.GetBan(_chatId);
            if(userblacklist != null)
            {
                _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                string reason = string.Empty;
                if(userblacklist.Reason == BlackListReason.Fraud)
                    reason = _localizer["You have been banned for fraud, contact support for more information."];
                else if (userblacklist.Reason == BlackListReason.Spam)
                    reason = _localizer["You have been banned for spamming the bot"];

                string caption =
                    $"🚷 {_localizer["You have been banned"]}\n" +
                    $"⚠️ {_localizer["Reason"]}: {Functions.EscapeMarkdownV2(reason)}\n";
                await _telegramService.SendText(_chatId, caption);
                return;
            }

            var userrepo = await RecordUser(_chatId, _firstName, _username, _languageCode);

            _selectedCountry = userrepo.SelectetdCountryName;

            if (input != null)
            {
                switch (input)
                {
                    case "/start":
                        _ = _telegramService.DeleteMessage(_chatId, _messageId);
                        await OnStart();
                        break;
                    case "/services":
                        _ = _telegramService.DeleteMessage(_chatId, _messageId);
                        await OnSelectServer();
                        break;
                    case "/activesms":
                    case "/5sim":
                        await _cacheService.Set($"{Strings.CURRENT_SERVER}:{_chatId}", input.Trim(), 1);
                        _ = _telegramService.DeleteMessage(_chatId, _messageId);
                        await OnServices();
                        break;
                    case "/my_profile":
                        _ = _telegramService.DeleteMessage(_chatId, _messageId);
                        await OnMyProfile();
                        break;
                    case "/previous":
                        await OnPreviousPageServices();
                        break;
                    case "/next":
                        await OnNextPageServices();
                        break;
                    case "/last":
                        await OnLastPageServices();
                        break;
                    case "/first":
                        await OnFirstPageServices();
                        break;
                    case "/add_balance":
                        _ = _telegramService.DeleteMessage(_chatId, _messageId);
                        await OnAddBalance();
                        break;
                    case "/add_voucher":
                        _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                        await OnVoucher();
                        break;
                    case "/listOldRentedSMS":
                        _ = _telegramService.DeleteMessage(_chatId, _messageId);
                        _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                        await OnListRentedNumberServices();
                        break;
                    case "/listOldRentedServicesSMS":
                        _ = _telegramService.DeleteMessage(_chatId, _messageId);
                        _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                        await OnListRentedSMSServices();
                        break;
                    case "increment_1":
                    case "decrement_1":
                    case "increment_5":
                    case "decrement_5":
                    case "increment_20":
                    case "decrement_20":
                        await Cart(input);
                        break;
                    case "/purchase":
                        _ = _telegramService.DeleteMessage(_chatId, _messageId);
                        await OnPurchase();
                        break;
                    case "/confirmrent":
                        await OnRentSmsService();
                        break;
                    case "/paises":
                        await OnListCoutries();
                        break;
                    default:
                        _ = _telegramService.DeleteMessage(_chatId, _messageId);
                        if (input.StartsWith(Strings.PREFIX_RENT_SMS_SERVICE))
                            await OnPreRentSmsService(input);
                        else if (input.StartsWith("/cancelrent"))
                        {
                            var existActivationId = input.Split(":").Length > 1;

                            if (existActivationId)
                            {
                                string activationId = input.Split(":")[1];

                                // TENTANDO CANCELAR
                                var currentServer = await _cacheService.Get($"{Strings.CURRENT_SERVER}:{_chatId}");
                                if (currentServer == "/activesms")
                                {
                                    var ativationcache = await _cacheService.Get($"{Strings.CURRENT_USER_RENT_SERVICE}:{activationId}");
                                    if (ativationcache != null)
                                    {
                                        var ativation = JsonConvert.DeserializeObject<CurrentSmsUserRentService>(ativationcache);
                                        var cancelSms = await _activeSmsService.UpdateStatusAtivationService(ativation.ActivationId, Domain.ActiveSms.StatusActivation.Cancel);
                                        if (!cancelSms)
                                        {
                                            RecurringJob.AddOrUpdate(ativation.ActivationId, () => SchreduleCancelAtivation(_chatId, ativation.ActivationId), "*/1 * * * *");
                                        }
                                        else
                                            RecurringJob.RemoveIfExists(ativation.ActivationId);

                                        /*var userbalance = await _userService.GetUserBalanceByChat(_chatId);

                                        userbalance.Balance += ativation.ValueRent;
                                        userbalance.Updated = DateTime.Now;
                                        await _userService.RecordBalance(userbalance);*/

                                        var history = await _userService.GetBalanceHistoryByTransactionId(ativation.ActivationId);
                                        history.Status = "canceled";
                                        history.CreatedAt = DateTime.Now;
                                        await _userService.RecordBalanceHistory(history);

                                        await _cacheService.Delete($"{Strings.PRE_RENT_DATA_SMS_SERVICE}{_chatId}");
                                    }
                                }
                                else if (currentServer == "/5sim")
                                {
                                    var ativationcache = await _cacheService.Get($"{Strings.CURRENT_USER_RENT_SERVICE}:{activationId}");
                                    var ativation = JsonConvert.DeserializeObject<CurrentSmsUserRentService>(ativationcache);
                                    RecurringJob.RemoveIfExists($"{_chatId}:{ativation.ActivationId}");

                                    await _fivesimService.CancelOrder(activationId);

                                    /*var userbalance = await _userService.GetUserBalanceByChat(_chatId);

                                    userbalance.Balance += ativation.ValueRent;
                                    userbalance.Updated = DateTime.Now;
                                    await _userService.RecordBalance(userbalance);*/

                                    var history = await _userService.GetBalanceHistoryByTransactionId(ativation.ActivationId);
                                    history.Status = "canceled";
                                    history.CreatedAt = DateTime.Now;
                                    await _userService.RecordBalanceHistory(history);

                                    await _cacheService.Delete($"{Strings.PRE_RENT_DATA_SMS_SERVICE}{_chatId}");
                                }
                            }

                            await OnServices();
                        }
                        else if (input.StartsWith("changecountry"))
                        {
                            string countryName = input.Split(":")[1];
                            var userrepository = await _userService.GetUserByChat(_chatId);
                            userrepository.SelectetdCountryName = countryName;
                            userrepository.UpdatedAt = DateTime.Now;
                            await _userService.Update(userrepository);
                            await OnStart();
                        }
                        else if (input.StartsWith("repurchaseNumber"))
                        {
                            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);

                            string caption = "";

                            await GetUserProfile();

                            var transationId = input.Split(":")[1];
                            var phoneNumber = input.Split(":")[2].Replace("+", "");
                            var currentServer = input.Split(":")[3];

                            await _cacheService.Set($"{Strings.CURRENT_SERVER}:{_chatId}", $"/{currentServer}", 1);

                            string activationId = "";

                            var userBalanceHistory = await _userService.GetBalanceHistoryByTransactionId(transationId);
                            var c = new CultureInfo(_languageCode);
                            string profit = userBalanceHistory.Amount.ToString("C", c);

                            if (userBalanceHistory.Amount > _userBalanceDouble)
                            {
                                caption = $"⛔️ {_localizer["Insufficient funds"]}" +
                                    $"💲 {_localizer["Service value"]}: {profit}";
                    
                                InlineKeyboardMarkup inlineKeyboard = new(new[]
                                {
                                        new []
                                        {
                                            optionAddBalance,
                                        },
                                        new []
                                        {
                                            optionStart,
                                        },
                                    });

                                await _telegramService.SendReplyMarkup(_chatId, caption, inlineKeyboard);
                                return;
                            }

                            async Task SendResponseUnavaliableRent()
                            {
                                caption =
                                        $"⚠️ {_localizer["Number unavailable for rent"]}\n" +
                                        $"😕 {_localizer["Try again later"]}";

                                InlineKeyboardMarkup inlineKeyboard = new(new[]
                                {
                                        // first row
                                        new []
                                        {
                                            optionStart,
                                        },
                                    });

                                await _telegramService.SendReplyMarkup(_chatId, caption, inlineKeyboard);
                            }

                            if (currentServer == "activesms")
                            {
                                var response = await _activeSmsService.RePurchaseNumber(transationId);

                                if(response == null)
                                {
                                    await SendResponseUnavaliableRent();
                                    return;
                                }

                                activationId = response.Split(":")[1];

                                caption =
                                    $"\U0001f977 {_localizer["Resend the SMS to get a new activation"]}\n" +
                                    $"📱 {_localizer["Service"]}: {userBalanceHistory.TransactionName.Split(":")[1]}\n\n" +
                                    $"💲 {_localizer["Value"]}: {profit}\n\n" +
                                    $"📞 {_localizer["Number"]}:\n{Functions.EscapeMarkdownV2($"{phoneNumber}", pre: true)} \n" +
                                    $"⏱ {_localizer["Waiting time"]}: 15 {_localizer["minutes"]}\n\n";
                            }
                            else if(currentServer == "5sim")
                            {
                                string product = userBalanceHistory.TransactionName.Split(":")[1];
                                var response = await _fivesimService.RePurchaseNumber(product, phoneNumber);
                                if (response == null)
                                {
                                    await SendResponseUnavaliableRent();
                                    return;
                                }

                                activationId = response.Id.ToString();
                                caption =
                                    $"\U0001f977 {_localizer["Resend the SMS to get a new activation"]}\n" +
                                    $"📱 {_localizer["Service"]}: {userBalanceHistory.TransactionName.Split(":")[1]}\n\n" +
                                    $"💲 {_localizer["Value"]}: {profit}\n\n" +
                                    $"📞 {_localizer["Number"]}:\n{Functions.EscapeMarkdownV2($"{phoneNumber}", pre: true)} \n" +
                                    $"⏱ {_localizer["Waiting time"]}: 15 {_localizer["minutes"]}\n\n";
                            }

                            async Task RegisterData(int messageId)
                            {
                                var userbalance = await _userService.GetUserBalanceByChat(_chatId);
                                userbalance.Balance -= userBalanceHistory.Amount;
                                userbalance.Updated = DateTime.Now;
                                await _userService.RecordBalance(userbalance);

                                var newuserBalanceHistory = new UserBalanceHistory();
                                newuserBalanceHistory.Guid = "";
                                newuserBalanceHistory.TransactionId = activationId;
                                newuserBalanceHistory.TransactionName = $"RE-PURCHASE_SERVICE:{userBalanceHistory.TransactionName.Split(":")[1]}";
                                newuserBalanceHistory.Status = "approved";
                                newuserBalanceHistory.Amount = userBalanceHistory.Amount;
                                newuserBalanceHistory.ChatId = _chatId;
                                newuserBalanceHistory.ConfirmedAt = DateTime.Now;
                                newuserBalanceHistory.CreatedAt = DateTime.Now;
                                newuserBalanceHistory.PhoneNumber = phoneNumber;
                                newuserBalanceHistory.Service = currentServer.Replace("/", "");
                                newuserBalanceHistory.TransationType = TransationType.RE_RENT;
                                string historyGuid = await _userService.RecordBalanceHistory(newuserBalanceHistory);

                                CurrentSmsUserRentService current = new();
                                current.ChatId = _chatId;
                                current.MessageId = messageId;
                                current.ActivationId = activationId;
                                current.PhoneNumber = phoneNumber;
                                current.BalanceHistoryId = historyGuid;
                                current.Value = profit;
                                current.ValueRent = userBalanceHistory.Amount;
                                current.RequestedAt = DateTime.Now;

                                await _cacheService.SetMinutes($"{Strings.CURRENT_USER_RENT_SERVICE}:{activationId}", JsonConvert.SerializeObject(current), 60);
                            }

                            var optionsCancelRent = InlineKeyboardButton.WithCallbackData($"❌ {_localizer["Cancel"]}", callbackData: $"/cancelrent:{activationId}");

                            var inlinekeyboard = new InlineKeyboardMarkup(new[] { optionsCancelRent });

                            int messageId = await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, inlinekeyboard);
                            await RegisterData(messageId);

                            if (currentServer == "activesms")
                            {
                                //RecurringJob.AddOrUpdate($"{_chatId}:{messageId}", () => SchreduleDeleteMessage(_chatId, messageId), "*/20 * * * *");
                            }
                            else
                            {
                                RecurringJob.AddOrUpdate($"{_chatId}:{activationId}", () => CheckSmsFiveSms(_chatId, activationId), "*/6 * * * * *");
                            }
                        }
                        else if (input.StartsWith("repurchaseService"))
                        {
                            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);

                            string caption = "";

                            await GetUserProfile();

                            var transationId = input.Split(":")[1];
                            var serviceKey = input.Split(":")[2];
                            var serviceName = input.Split(":")[3];
                            var currentServer = input.Split(":")[4];

                            await _cacheService.Set($"{Strings.CURRENT_SERVER}:{_chatId}", $"/{currentServer}", 1);

                            //var costObject = await _activeSmsService.GetCurrentPriceByService(serviceKey, _selectedCountry);

                            //if(costObject == null)
                            //{
                            //    return;
                            //}

                            //var property = costObject.Properties().First();
                            //string key = property.Name;

                            //var jsonService = JsonConvert.SerializeObject(costObject[key]);
                            //var objService = JObject.Parse(jsonService);
                            //var keyService = objService.Properties().First().Name;

                            //var service = objService[keyService];
                            //var jsonCost = JsonConvert.SerializeObject(objService[keyService]);
                            //JObject costObj = JObject.Parse(jsonCost);

                            string callbackData = string.Empty;
                            var currentForex = await _forexService.GetForex("BRL", "RUB");

                            if (currentServer == "activesms")
                            {
                                ActiveSmsServices services = await _activeSmsService.GetServicesAndPrices(_selectedCountry);

                                if (services == null || services.Services.Count == 0)
                                {
                                    await OnStart();
                                    string msg =
                                        $"⛔️ {_localizer["There are no numbers available at the moment, please try again in 60 minutes"]}";

                                    await _telegramService.SendText(_chatId, msg);
                                    await _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                                    await _cacheService.Delete($"{_chatId}{Strings.SERVICES_PAGINATION}");
                                    return;
                                }

                                var orderedServices = services.Services
                                .OrderBy(service => servicesNamesForRenames.ContainsKey(service.Key) ? servicesNamesForRenames[service.Key] : service.Value.SearchName)
                                .ToList();

                                var service = orderedServices.Where(x => x.Key == serviceKey).FirstOrDefault();
                                
                                double rubRentalCost = service.Value.RetailCost;
                                double rentalCost = rubRentalCost / currentForex;

                                double profitBrl = (rentalCost * int.Parse(_configuration["Resale:ProfitFlagTwo"]) / 100);

                                double total = profitBrl + rentalCost;
                                if (total > 5)
                                {
                                    double disccount = profitBrl * int.Parse(_configuration["Resale:Profit"]) / 100;
                                    total -= disccount;
                                }

                                #region FIXED PRICES

                                total = MakeFixedPrice(service.Key, total);

                                #endregion

                                var c = new CultureInfo(_languageCode);
                                string profitFormated = total.ToString("C2", c);

                                callbackData = $"{Strings.PREFIX_RENT_SMS_SERVICE}{serviceKey}:{serviceName}:{profitFormated}:{total}:{rubRentalCost}";
                            }
                            else if (currentServer == "5sim")
                            {
                                if (_selectedCountry.ToLower().Replace(" ", "") == "unitedstates")
                                    _selectedCountry = "usa";

                                var services = await _fivesimService.GetServicesAndPrices(_selectedCountry);

                                if (services == null || services.Count == 0)
                                {
                                    await OnStart();
                                    string msg =
                                        $"⛔️ {_localizer["There are no numbers available at the moment, please try again in 60 minutes"]}";

                                    await _telegramService.SendText(_chatId, msg);
                                    await _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                                    await _cacheService.Delete($"{_chatId}{Strings.SERVICES_PAGINATION}");
                                    return;
                                }

                                var service = services.Where(x => x.Key == serviceKey).First();

                                double rubRentalCost = service.Value.Price;
                                double rentalCost = rubRentalCost / currentForex;

                                double profitBrl = (rentalCost * (int.Parse(_configuration["Resale:ProfitFlagTwo"]) + int.Parse(_configuration["Resale:ProfitFlagThree"])) / 100);

                                double total = profitBrl + rentalCost;
                                if (total > 5)
                                {
                                    double disccount = profitBrl * int.Parse(_configuration["Resale:Profit"]) / 100;
                                    total -= disccount;
                                }

                                var c = new CultureInfo(_languageCode);
                                string profitFormated = total.ToString("C2", c);

                                string serviceNameFormmated = char.ToUpper(service.Key[0]) + service.Key[1..].ToLower();
                                callbackData = $"{Strings.PREFIX_RENT_SMS_SERVICE}{service.Key}:{profitFormated}:{total}:{service.Value.Price}";
                            }

                            
                            await OnPreRentSmsService(callbackData);
                        }
                        else if (input.Trim().StartsWith("/NBV"))
                        {
                            string voucher = input.Trim();

                            var uservoucher = await _userVoucherRepository.GetByCodeAndChatId(voucher, _chatId);
                            if (uservoucher != null)
                            {
                                await _telegramService.SendText(_chatId, $"⚠️ {_localizer["You have already used this voucher"]}");
                                return;
                            }
                            var voucherrepo = await _voucherRepository.GetByCode(voucher);
                            if (voucherrepo == null)
                            {
                                await _telegramService.SendText(_chatId, $"⚠️ {_localizer["Invalid voucher"]}");
                                return;
                            }

                            if (voucherrepo.Quantity == 0)
                            {
                                await _telegramService.SendText(_chatId, $"⚠️ {_localizer["All vouchers have already been used"]}");
                                return;
                            }

                            if(voucherrepo.CreatedAt.AddHours(24) < DateTime.Now)
                            {
                                await _telegramService.SendText(_chatId, $"⚠️ {_localizer["Voucher expired"]}");
                                return;
                            }

                            var userBalanceHistory = await _userService.GetBalanceHistoryByChatIdAndStatus(_chatId, TransationType.VOUCHER);
                            if (userBalanceHistory != null)
                            {
                                var approveds = userBalanceHistory
                                                .Where(x => x.Status == "approved" && x.CreatedAt >= DateTime.Now.AddDays(-3))
                                                .ToList();

                                if(approveds.Count > 1)
                                {
                                    await _telegramService.SendText(_chatId, $"⚠️ {_localizer["You have redeemed many vouchers in the last 3 days"]}");
                                    return;
                                }
                            }

                            voucherrepo.Quantity -= 1;
                            await _voucherRepository.Update(voucherrepo);

                            uservoucher = new();
                            uservoucher.ChatId = _chatId;
                            uservoucher.Code = voucher;
                            uservoucher.UsageDate = DateTime.Now;
                            await _userVoucherRepository.Record(uservoucher);

                            var userbalance = await _userService.GetUserBalanceByChat(_chatId);
                            userbalance ??= new();
                            userbalance.ChatId = _chatId;
                            userbalance.Balance += voucherrepo.Amount;
                            userbalance.Updated = DateTime.Now;
                            await _userService.RecordBalance(userbalance);

                            var history = new UserBalanceHistory();
                            history.Status = "voucher";
                            history.TransactionId = uservoucher.Code;
                            history.TransactionName = $"VOUCHER";
                            history.Status = "approved";
                            history.Amount = voucherrepo.Amount;
                            history.ChatId = _chatId;
                            history.ConfirmedAt = DateTime.Now;
                            history.CreatedAt = DateTime.Now;
                            history.TransationType = TransationType.VOUCHER;
                            await _userService.RecordBalanceHistory(history);

                            await _telegramService.SendText(_chatId, $"✅ {_localizer["Voucher added successfully"]}");
                            _ = OnStart();

                            var c = new CultureInfo(_languageCode);
                            var value = voucherrepo.Amount.ToString("C", c);

                            string caption = Functions.EscapeMarkdownV2($"♻️ {_firstName} {_localizer["Retrieved a voucher"]}\n") +
                                 $"🔖 {_localizer["Code"]} {Functions.EscapeMarkdownV2(voucher)}\n" +
                                 $"💰 {_localizer["Value"]}: {value}\n\n" +
                                 $"\U0001f977 {_localizer["Buy with ninja bot"]}\n" +
                                 $"👇 {Functions.EscapeMarkdownV2(_configuration["Telegram:BotName"])}";

                            _ = SendMessageToChannel(caption);
                        }
                        else if (input.Trim().StartsWith("/admin"))
                        {
                            var commandSplit = input.Split(".");
                            if (commandSplit.Length < 2) return;

                            if (commandSplit[1] != _configuration["SecretVoucher"]) return;

                            if (commandSplit[2] == "addvoucher")
                            {
                                int quantity = int.Parse(commandSplit[3]);
                                string code = $"/NBV{commandSplit[4]}";

                                Voucher voucher = new();
                                voucher.Guid = Guid.NewGuid().ToString();
                                voucher.Code = code;
                                voucher.Quantity = int.Parse(commandSplit[3]);
                                voucher.Amount = int.Parse(commandSplit[4]) / 100.0;
                                voucher.CreatedAt = DateTime.Now;
                                await _voucherRepository.Record(voucher);

                                await _telegramService.SendText(_chatId, code);
                            }
                            else if (commandSplit[2] == "userban")
                            {
                                var reason = BlackListReason.Fraud;
                                if(commandSplit[3] == "fraud")
                                    reason = BlackListReason.Fraud;
                                else if (commandSplit[3] == "spam")
                                    reason = BlackListReason.Spam;

                                await _userService.BanUser(commandSplit[4], reason);
                            }
                            else if (commandSplit[2] == "userunban")
                            {
                                await _userService.UnBanUser(commandSplit[3]);
                            }
                        }
                        break;
                }
            }
        }

        protected async Task<User> RecordUser(string chatId, string name, string userName, string language)
        {
            var user = await _userService.GetUserByChat(chatId);
            if (user == null)
            {
                user = new();
                user.ChatId = chatId;
                user.ContactId = chatId;
                user.Name = name;
                user.Username = userName;
                user.Language = language;
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;

                var c = new CultureInfo(language);
                var r = new RegionInfo(c.LCID);
                user.SelectetdCountryName = r.EnglishName;

                await _userService.Record(user);
            }

            return user;
        }

        protected async Task GetUserProfile(string? chatId = null)
        {
            var user = await _userService.GetUserByChat(chatId ?? _chatId);

            _selectedCountry = user.SelectetdCountryName;

            var userbalance = await _userService.GetUserBalanceByChat(chatId ?? _chatId);
            var c = new CultureInfo(user.Language);
            double balance = 0.0;
            if (userbalance != null)
                balance = userbalance.Balance;

            _userBalanceDouble = balance;
            _userBalance = balance.ToString("C", c);
        }

        protected async Task OnStart()
        {
            await GetUserProfile();

            //var c = new CultureInfo(_languageCode);
            //var r = new RegionInfo(c.LCID);
            //var countrycode = r.TwoLetterISORegionName.ToLower();

            string caption = Functions.EscapeMarkdownV2($"\U0001f977 {Functions.ReplaceVariable(_localizer["Hello, {{name}}! Welcome to!"], "name", _firstName)}\n\n" +
                 $"🥷 {_localizer["I'm your digital ninja to send SMS from different apps and services"]}\n\n" +
                 $"📢 {_localizer["Don't forget to join our channel and invite your friends, the more friends you have, the more vouchers are released on our channel!"]}\n\n" +
                 $"📍 {_localizer["Country"]}: {_selectedCountry} [/{_localizer["countrys"]}]\n" +
                 $"💰 {_localizer["Your balance"]}: {Functions.EscapeMarkdownV2(_userBalance)}\n\n" +
                 $"👇 {_localizer["Options Below"]}");

            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                // first row
                new []
                {
                    optionServices,
                },
                // second row
                new []
                {
                    optionAddBalance,
                },
                new[]
                {
                    optionsListOldServicesRented,
                    optionsListOldNumberRented
                },
                new []
                {
                    optionAddVoucher,
                },
                // second row
                new []
                {
                    optionMyProfile,
                    optionSupport,
                },
                 new []
                {
                    optionMyPolices,
                    optionMyGroups,
                },
            });

            await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, inlineKeyboard);

            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
        }

        protected async Task OnSelectServer()
        {
            await GetUserProfile();

            string caption = Functions.EscapeMarkdownV2($"\U0001f977 {_firstName}! {_localizer["Choose which server you want to receive SMS"]}\n\n" +
                 $"🌍 {_localizer["Country"]}: {_selectedCountry}\n" +
                 $"💰 {_localizer["Your balance"]}: {Functions.EscapeMarkdownV2(_userBalance)}\n\n" +
                 $"👇 {_localizer["Options Below"]}");

            InlineKeyboardMarkup inlineKeyboard = new(new[]
           {
                // first row
                new []
                {
                    optionServerSmsActive,
                    optionServer5Sim
                },
                // second row
                new []
                {
                    optionStart,
                },
            });

            await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, inlineKeyboard);
        }

        protected async Task OnServices()
        {
            //var services = await _fivesimService.GetServicesAndPrices("Brazil");
            var currentForex = await _forexService.GetForex("BRL", "RUB");
            List<InlineKeyboardButton[][]> buttonArraysChunks = new();
            async Task GetServices()
            {
                async Task SmsActive()
                {
                    ActiveSmsServices services = await _activeSmsService.GetServicesAndPrices(_selectedCountry);

                    if (services == null || services.Services.Count == 0)
                    {
                        await OnStart();
                        string msg =
                            $"⛔️ {_localizer["There are no numbers available at the moment, please try again in 60 minutes"]}";

                        await _telegramService.SendText(_chatId, msg);
                        await _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                        await _cacheService.Delete($"{_chatId}{Strings.SERVICES_PAGINATION}");
                        return;
                    }

                    List<InlineKeyboardButton[]> buttonArrays = new List<InlineKeyboardButton[]>();
                    List<InlineKeyboardButton> tempArray = new List<InlineKeyboardButton>();

                    var orderedServices = services.Services
                    .OrderBy(service => servicesNamesForRenames.ContainsKey(service.Key) ? servicesNamesForRenames[service.Key] : service.Value.SearchName)
                    .ToList();

                    foreach (var service in orderedServices)
                    {
                        if (servicesToHide.Contains(service.Key)) continue;

                        string formatedName = servicesNamesForRenames.ContainsKey(service.Key) ? servicesNamesForRenames[service.Key] : service.Value.SearchName;
                        string serviceName = servicesNamesForRenames.ContainsKey(service.Key) ? servicesNamesForRenames[service.Key] : service.Value.SearchName;

                        double rubRentalCost = service.Value.RetailCost;

                        double rentalCost = rubRentalCost / currentForex;

                        double profitBrl = (rentalCost * int.Parse(_configuration["Resale:ProfitFlagTwo"]) / 100);

                        double total = profitBrl + rentalCost;
                        if (total > 5)
                        {
                            double disccount = profitBrl * int.Parse(_configuration["Resale:Profit"]) / 100;
                            total -= disccount;
                        }

                        #region FIXED PRICES

                        total = MakeFixedPrice(service.Key, total);

                        #endregion

                        var c = new CultureInfo(_languageCode);
                        string profitFormated = total.ToString("C2", c);

                        string callbackData = $"{Strings.PREFIX_RENT_SMS_SERVICE}{service.Key}:{serviceName}:{profitFormated}:{total}:{service.Value.RetailCost}";
                        int byteCount = System.Text.Encoding.UTF8.GetByteCount(callbackData);
                        if (byteCount <= 64)
                        {
                            tempArray.Add(InlineKeyboardButton.WithCallbackData(text: $"{formatedName} {profitFormated}", callbackData: callbackData.Normalize()));

                            if (tempArray.Count == 2)
                            {
                                buttonArrays.Add(tempArray.ToArray());
                                tempArray.Clear();
                            }
                        }
                    }

                    if (tempArray.Count > 0)
                    {
                        buttonArrays.Add(tempArray.ToArray());
                    }

                    // Divida os botões em grupos de 20 usando LINQ
                    buttonArraysChunks = buttonArrays
                        .Select((value, index) => new { value, index })
                        .GroupBy(x => x.index / 6)
                        .Select(group => group.Select(x => x.value).ToArray())
                        .ToList();
                };

                async Task FiveSim()
                {
                    if (_selectedCountry.ToLower().Replace(" ", "") == "unitedstates")
                        _selectedCountry = "usa";

                    var services = await _fivesimService.GetServicesAndPrices(_selectedCountry);

                    List<InlineKeyboardButton[]> buttonArrays = new List<InlineKeyboardButton[]>();
                    List<InlineKeyboardButton> tempArray = new List<InlineKeyboardButton>();

                    if (services == null || (services != null && services.Count == 0))
                    {
                        await OnStart();
                        string msg =
                            $"⛔️ {_localizer["There are no numbers available at the moment, please try again in 60 minutes"]}";

                        await _telegramService.SendText(_chatId, msg);
                        await _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                        await _cacheService.Delete($"{_chatId}{Strings.SERVICES_PAGINATION}");
                        return;
                    }

                    foreach (var service in services)
                    {
                        //if (servicesToHide.Contains(service.Key)) continue;

                        double rubRentalCost = service.Value.Price;
                        double rentalCost = rubRentalCost / currentForex;

                        double profitBrl = (rentalCost * (int.Parse(_configuration["Resale:ProfitFlagTwo"]) + int.Parse(_configuration["Resale:ProfitFlagThree"])) / 100);

                        double total = profitBrl + rentalCost;
                        if (total > 5)
                        {
                            double disccount = profitBrl * int.Parse(_configuration["Resale:Profit"]) / 100;
                            total -= disccount;
                        }

                        var c = new CultureInfo(_languageCode);
                        string profitFormated = total.ToString("C2", c);

                        string serviceNameFormmated = char.ToUpper(service.Key[0]) + service.Key[1..].ToLower();
                        string callbackData = $"{Strings.PREFIX_RENT_SMS_SERVICE}{service.Key}:{profitFormated}:{total}:{service.Value.Price}";
                        int byteCount = System.Text.Encoding.UTF8.GetByteCount(callbackData);
                        if (byteCount <= 64)
                        {
                            tempArray.Add(InlineKeyboardButton.WithCallbackData(text: $"{serviceNameFormmated} {profitFormated}", callbackData: callbackData.Normalize()));

                            if (tempArray.Count == 2)
                            {
                                buttonArrays.Add(tempArray.ToArray());
                                tempArray.Clear();
                            }
                        }
                    }

                    if (tempArray.Count > 0)
                    {
                        buttonArrays.Add(tempArray.ToArray());
                    }

                    // Divida os botões em grupos de 20 usando LINQ
                    buttonArraysChunks = buttonArrays
                        .Select((value, index) => new { value, index })
                        .GroupBy(x => x.index / 6)
                        .Select(group => group.Select(x => x.value).ToArray())
                        .ToList();

                };

                var currentServer = await _cacheService.Get($"{Strings.CURRENT_SERVER}:{_chatId}");
                if (currentServer == null)
                    await SmsActive();
                else if (currentServer == "/activesms")
                    await SmsActive();
                else if (currentServer == "/5sim")
                    await FiveSim();
            }

            await GetServices();

            await _cacheService.Set($"{_chatId}{Strings.SERVICES_PAGINATION}", JsonConvert.SerializeObject(buttonArraysChunks), 1);
            await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_PAGE}", "0", 1);

            await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_SERVICES_PAGINATION}", "/services", 1); // INDICA QUAL OBJETO ESTÁ NA PAGINACAO DO CACHE

            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
            var btsWithPagination = AddPaginationInKeyboardButtonTelegram(buttonArraysChunks[0].ToList());
            InlineKeyboardMarkup inlineKeyboardMarkup = new(btsWithPagination);

            await GetUserProfile();

            string caption = Functions.EscapeMarkdownV2($"\U0001f977 {_firstName}! {_localizer["Choose which service you want to receive SMS"]}\n\n" +
                 $"🌍 {_localizer["Country"]}: {_selectedCountry}\n" +
                 $"💰 {_localizer["Your balance"]}: {Functions.EscapeMarkdownV2(_userBalance)}\n\n" +
                 $"👇 {_localizer["Options Below"]}");

            await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, inlineKeyboardMarkup);
        }

        protected async Task OnAddBalance()
        {
            await _cacheService.Delete($"{Strings.USER_CART}_{_chatId}");

            Cart cart = new();
            cart.ChatId = _chatId;
            await _cacheService.Set($"{Strings.USER_CART}_{_chatId}", JsonConvert.SerializeObject(cart), 1);

            await GetUserProfile();

            string caption =
                $"{Functions.EscapeMarkdownV2($"\U0001f977 {_firstName}!")} {_localizer["Choose how much you want to add to your balance"]}\n\n" +
                $"💰 {_localizer["Your balance"]}: {Functions.EscapeMarkdownV2(_userBalance)}\n\n" +
                $"💵 {_localizer["Value"]}: {cart.CurrentValue}";

            await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, CartActions());
            await _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
        }

        protected async Task OnVoucher()
        {
            string message = $"🔖 {_localizer["Enter voucher code"]}";
            await _telegramService.SendText(_chatId, message);
        }

        protected async Task OnListRentedNumberServices()
        {
            var rentedServices = await _userService.GetBalanceHistoryByChatIdAndStatus(_chatId, "approved", TransationType.RENT);

            string caption;
            if (rentedServices == null || rentedServices.Count == 0)
            {
                caption = Functions.EscapeMarkdownV2($"🚨 {_firstName} {_localizer["You do not yet have leased numbers that have successfully received SMS"]}\n\n" +
                 $"⚠️ {_localizer["To re-purchase a number you must have previously received an SMS"]}\n");

                InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    // first row
                    new []
                    {
                        optionStart,
                    },
                });

                await _telegramService.SendReplyMarkup(_chatId, caption, inlineKeyboard);
                return;
            }
            List<InlineKeyboardButton[][]> buttonArraysChunks = new();
            List<InlineKeyboardButton> numbersBtn = new List<InlineKeyboardButton>();
            List<InlineKeyboardButton[]> buttonArrays = new List<InlineKeyboardButton[]>();

            foreach (var balanceHistory in rentedServices)
            {
                if (balanceHistory.PhoneNumber == null) continue;

                numbersBtn.Add(InlineKeyboardButton.WithCallbackData(text: Functions.EscapeMarkdownV2($"{balanceHistory.PhoneNumber.Replace("+", "")}"), 
                    callbackData: $"repurchaseNumber:{balanceHistory.TransactionId}:{balanceHistory.PhoneNumber}:{balanceHistory.Service}"));
                
                if (numbersBtn.Count == 2)
                {
                    buttonArrays.Add(numbersBtn.ToArray());
                    numbersBtn.Clear();
                }
            }

            if (numbersBtn.Count > 0)
                buttonArrays.Add(numbersBtn.ToArray());

            // Divida os botões em grupos de 20 usando LINQ
            buttonArraysChunks = buttonArrays
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / 4)
                .Select(group => group.Select(x => x.value).ToArray())
                .ToList();

            caption =
                 $"🥷 {Functions.EscapeMarkdownV2(_localizer["Choose which number you want to repurchase"])}\n\n" +
                 $"👇 {_localizer["Numbers below"]}";

            await _cacheService.Set($"{_chatId}{Strings.SERVICES_PAGINATION}", JsonConvert.SerializeObject(buttonArraysChunks), 1);
            await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_PAGE}", "0", 1);

            await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_SERVICES_PAGINATION}", "/repurchase", 1); // INDICA QUAL OBJETO ESTÁ NA PAGINACAO DO CACHE

            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);

            InlineKeyboardMarkup inlineKeyboardMarkup;
            if (buttonArraysChunks.Count > 6)
            {
                var btsWithPagination = AddPaginationInKeyboardButtonTelegram(buttonArraysChunks[0].ToList());
                inlineKeyboardMarkup = new(btsWithPagination);
            }
            else
            {
                var toList = buttonArraysChunks[0].ToList();
                toList.Add(new InlineKeyboardButton[] { optionStart });
                inlineKeyboardMarkup = new(toList);
            }

            await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, inlineKeyboardMarkup);
        }

        protected async Task OnListRentedSMSServices()
        {
            var rentedServices = await _userService.GetBalanceHistoryByChatIdAndStatus(_chatId, TransationType.RENT);

            rentedServices = rentedServices.Where(x => x.ServiceKey != string.Empty).DistinctBy(x => x.ServiceKey).ToList();

            string caption;
            if (rentedServices == null || rentedServices.Count == 0)
            {
                caption = Functions.EscapeMarkdownV2($"🚨 {_firstName} {_localizer["You haven't ordered any apps yet, click Receive SMS to make a quick purchase later"]}\n");

                InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    // first row
                    new []
                    {
                        optionServices,
                    },
                    new []
                    {
                        optionStart,
                    },
                });

                await _telegramService.SendReplyMarkup(_chatId, caption, inlineKeyboard);
                return;
            }
            List<InlineKeyboardButton[][]> buttonArraysChunks = new();
            List<InlineKeyboardButton> numbersBtn = new List<InlineKeyboardButton>();
            List<InlineKeyboardButton[]> buttonArrays = new List<InlineKeyboardButton[]>();

            foreach (var balanceHistory in rentedServices)
            {
                string serviceName = balanceHistory.TransactionName.Split(':').Last().Trim();
                string server = balanceHistory.Service == "activesms" ? "SR1" : "SR2";

                numbersBtn.Add(InlineKeyboardButton.WithCallbackData(text: $"{server}:{Functions.EscapeMarkdownV2($"{serviceName}")}",
                    callbackData: $"repurchaseService:{balanceHistory.TransactionId}:{balanceHistory.ServiceKey}:{serviceName}:{balanceHistory.Service}"));

                if (numbersBtn.Count == 2)
                {
                    buttonArrays.Add(numbersBtn.ToArray());
                    numbersBtn.Clear();
                }
            }

            if (numbersBtn.Count > 0)
                buttonArrays.Add(numbersBtn.ToArray());

            // Divida os botões em grupos de 20 usando LINQ
            buttonArraysChunks = buttonArrays
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / 4)
                .Select(group => group.Select(x => x.value).ToArray())
                .ToList();

            caption =
                 $"🥷 {Functions.EscapeMarkdownV2(_localizer["Choose which APP you want to reactivate"])}\n\n" +
                 $"👇 {_localizer["APPs below"]}";

            await _cacheService.Set($"{_chatId}{Strings.SERVICES_PAGINATION}", JsonConvert.SerializeObject(buttonArraysChunks), 1);
            await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_PAGE}", "0", 1);

            await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_SERVICES_PAGINATION}", "/repurchaseApp", 1); // INDICA QUAL OBJETO ESTÁ NA PAGINACAO DO CACHE

            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);

            InlineKeyboardMarkup inlineKeyboardMarkup;
            if (buttonArraysChunks.Count > 6)
            {
                var btsWithPagination = AddPaginationInKeyboardButtonTelegram(buttonArraysChunks[0].ToList());
                inlineKeyboardMarkup = new(btsWithPagination);
            }
            else
            {
                var toList = buttonArraysChunks[0].ToList();
                toList.Add(new InlineKeyboardButton[] { optionStart });
                inlineKeyboardMarkup = new(toList);
            }

            await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, inlineKeyboardMarkup);
        }

        protected async Task OnPurchase()
        {
            var cartcache = await _cacheService.Get($"{Strings.USER_CART}_{_chatId}");
            if (cartcache != null)
            {
                Cart cart = JsonConvert.DeserializeObject<Cart>(cartcache);

                var payment = await _paymentService.GeneratePix(_chatId, cart.CurrentValue);

                string caption =
                    $"{Functions.EscapeMarkdownV2("\U0001f977")} {_localizer["Your order has been created"]}\n" +
                    $"{Functions.EscapeMarkdownV2("\U0001f977")} {_localizer["PIX copy and paste"]}\n\n" +
                    $"👇 {_localizer["Click on the link below to copy and paste into your bank to make the payment"]} 👇\n\n" +
                    $"{Functions.EscapeMarkdownV2(payment.PointOfInteraction.TransactionData.QrCode, code: true)}\n\n" +
                    $"⚠️ {_localizer["The generated code is not the random key, but rather a copy and paste code for the pix function"]}\n\n" +
                    $"♻️ {_localizer["After payment, the balance will be added to your account automatically"]}\n\n" +
                    $"{Functions.EscapeMarkdownV2("\U0001f977")} {_localizer["Link expires in 15 minutes"]}\n" +
                    $"{Functions.EscapeMarkdownV2("\U0001f977")} {_localizer["To copy CLICK on the link"]}\n" +
                    $"{Functions.EscapeMarkdownV2("\U0001f977")} {_localizer["Read before recharging"]} 👉 /policys";


                await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, new InlineKeyboardMarkup(new[] { optionStart }));
            }
            else
                await OnAddBalance();
        }

        protected async Task OnMyProfile()
        {
            await GetUserProfile();

            var c = new CultureInfo(_languageCode);
            var r = new RegionInfo(c.LCID);
            var countrycode = r.TwoLetterISORegionName.ToLower();

            string caption =
                $"\U0001f977 {Functions.EscapeMarkdownV2($"{_localizer["Account information"]}", bold: true)}\n\n" +
                $"\U0001f977 {_localizer["Id"]}: {Functions.EscapeMarkdownV2(_chatId, code: true)}\n" +
                $"\U0001f977 {_localizer["User"]}: {Functions.EscapeMarkdownV2($"@{_username}", code: true)} \n" +
                $"\U0001f977 {_localizer["Name"]}: {Functions.EscapeMarkdownV2($"{_firstName}", code: true)} \n" +
                $"\U0001f977 {_localizer["Country"]}: {Functions.IsoCountryCodeToFlagEmoji(countrycode)} {r.EnglishName} \n" +
                $"\U0001f977 {_localizer["Your balance"]}: {Functions.EscapeMarkdownV2(_userBalance, code: true)}";

            await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, new InlineKeyboardMarkup(new[] { optionStart }));
            await _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
        }

        protected async Task OnPreRentSmsService(string input)
        {
            await GetUserProfile();

            string servicecode = string.Empty;
            string service = string.Empty;
            string profit = string.Empty;
            double valueRent = 0.0;
            string serviceName = string.Empty;
            string rentalCost = string.Empty;

            var currentServer = await _cacheService.Get($"{Strings.CURRENT_SERVER}:{_chatId}");
            if (currentServer == null || currentServer == "/activesms")
            {
                servicecode = input.Split(":")[1];
                service = input.Split(":")[2];
                profit = input.Split(":")[3];
                valueRent = double.Parse(input.Split(":")[4]);
                rentalCost = input.Split(":")[5];

                serviceName = servicesNamesForRenames.ContainsKey(servicecode) ? servicesNamesForRenames[servicecode] : service;
            }
            else
            {
                servicecode = input.Split(":")[1];
                profit = input.Split(":")[2];
                valueRent = double.Parse(input.Split(":")[3]);
                rentalCost = input.Split(":")[4];

                serviceName = char.ToUpper(servicecode[0]) + servicecode[1..].ToLower();
            }

            string caption =
                        $"🌍 {_localizer["Country"]}: {_selectedCountry}\n\n" +
                        $"📱 {_localizer["Service"]}: {serviceName}\n\n" +
                        $"💲 {_localizer["Value"]}: {profit}\n\n" +
                        $"💰 {_localizer["Your balance"]}: {Functions.EscapeMarkdownV2(_userBalance)}\n\n";

            InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    new []
                    {
                        _userBalanceDouble < valueRent ? optionAddBalance : optionsConfirmRent
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"❌ {_localizer["Cancel"]}", callbackData: $"/cancelrent")
                    },
                });

            PreRentSms preRentSms = new()
            {
                ServiceCode = servicecode,
                ServiceName = serviceName,
                Value = profit,
                ValueRent = valueRent,
                RentalCost = rentalCost
            };
            await _cacheService.SetMinutes($"{Strings.PRE_RENT_DATA_SMS_SERVICE}{_chatId}", JsonConvert.SerializeObject(preRentSms), 10);

            await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, inlineKeyboard);
            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
        }

        protected async Task OnRentSmsService()
        {
            await GetUserProfile();

            var rentDataJson = await _cacheService.Get($"{Strings.PRE_RENT_DATA_SMS_SERVICE}{_chatId}");
            if (rentDataJson == null)
            {
                string expired =
                    $"⛔️ {_localizer["This request has expired"]}";

                await _telegramService.SendText(_chatId, expired);
                await _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                await OnServices();
                return;
            }
            var rentData = JsonConvert.DeserializeObject<PreRentSms>(rentDataJson);
            if (_userBalanceDouble < rentData.ValueRent)
            {
                await OnServices();
                string msg =
                    $"⛔️ {_localizer["Insufficient funds"]}";

                await _telegramService.SendText(_chatId, msg);
                await _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                return;
            }

            async Task VericationIsNull(object service)
            {
                if (service == null)
                {
                    string msg =
                        $"🌍 {_localizer["Country"]}: {_selectedCountry}\n\n" +
                        $"📱 {_localizer["Service"]}: {rentData.ServiceName}\n\n" +
                        $"💲 {_localizer["Value"]}: {rentData.Value}\n\n" +
                        $"💰 {_localizer["Your balance"]}: {Functions.EscapeMarkdownV2(_userBalance)}\n\n" +
                        $"⛔️ {_localizer["There are no numbers available at the moment, please try again in 60 minutes"]}";

                    InlineKeyboardMarkup inlineKeyboard = new(new[]
                        {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"🔄 {_localizer["Try again"]}", callbackData: "/confirmrent")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"❌ {_localizer["Cancel"]}", callbackData: $"/cancelrent")
                    },
                });

                    _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
                    _ = _telegramService.EditCaption(_chatId, _messageId, msg, inlineKeyboard);
                    return;
                }
            }

            dynamic service = null;

            var currentServer = await _cacheService.Get($"{Strings.CURRENT_SERVER}:{_chatId}");
            string caption = "";
            string proneNumber;
            if (string.IsNullOrEmpty(currentServer) || currentServer == "/activesms")
            {
                currentServer ??= "/activesms";
                // Request service ActiveSms
                service = await _activeSmsService.RequestAtivationService(rentData.ServiceCode, _selectedCountry, rentData.RentalCost);
                await VericationIsNull(service);

                proneNumber = service.PhoneNumber;

                caption =
                    $"\U0001f977 {_localizer["Your number is below"]}\n" +
                    $"🌍 {_localizer["Country"]}: {_selectedCountry}\n" +
                    $"📱 {_localizer["Service"]}: {rentData.ServiceName}\n\n" +
                    $"📞 {_localizer["Number"]}:\n{Functions.EscapeMarkdownV2($"{service.PhoneNumber}", pre: true)} \n" +
                    $"⏱ {_localizer["Waiting time"]}: 15 {_localizer["minutes"]}\n\n";
            }
            else
            {
                service = await _fivesimService.RequestAtivationService(_selectedCountry, rentData.ServiceCode, rentData.RentalCost);
                await VericationIsNull(service);

                proneNumber = service.Phone;

                caption =
                    $"\U0001f977 {_localizer["Your number is below"]}\n" +
                    $"🌍 {_localizer["Country"]}: {_selectedCountry}\n" +
                    $"📱 {_localizer["Service"]}: {rentData.ServiceName}\n\n" +
                    $"📞 {_localizer["Number"]}:\n{Functions.EscapeMarkdownV2($"{service.Phone}", pre: true)} \n" +
                    $"⏱ {_localizer["Waiting time"]}: 15 {_localizer["minutes"]}\n\n";
            }

            await _telegramService.DeleteMessage(_chatId, _messageId);

            string ativationId = currentServer == "/activesms" ? service.ActivationId : $"{service.Id}";

            async Task RegisterData(int messageId)
            {
                /*var userbalance = await _userService.GetUserBalanceByChat(_chatId);
                userbalance.Balance -= rentData.ValueRent;
                userbalance.Updated = DateTime.Now;
                await _userService.RecordBalance(userbalance);*/

                var userBalanceHistory = new UserBalanceHistory();
                userBalanceHistory.TransactionId = ativationId;
                userBalanceHistory.TransactionName = $"PURCHASE_SERVICE:{rentData.ServiceName}";
                userBalanceHistory.ServiceKey = rentData.ServiceCode;
                userBalanceHistory.Country = _selectedCountry;
                userBalanceHistory.Status = "approved";
                userBalanceHistory.Amount = rentData.ValueRent;
                userBalanceHistory.ChatId = _chatId;
                userBalanceHistory.ConfirmedAt = DateTime.Now;
                userBalanceHistory.CreatedAt = DateTime.Now;
                userBalanceHistory.PhoneNumber = proneNumber;
                userBalanceHistory.Service = currentServer.Replace("/", "");
                userBalanceHistory.TransationType = TransationType.RENT;
                string historyGuid = await _userService.RecordBalanceHistory(userBalanceHistory);

                CurrentSmsUserRentService current = new();
                current.ChatId = _chatId;
                current.MessageId = messageId;
                current.ActivationId = ativationId;
                current.PhoneNumber = currentServer == "/activesms" ? service.PhoneNumber : service.Phone;
                current.BalanceHistoryId = historyGuid;
                current.Value = rentData.Value;
                current.ValueRent = rentData.ValueRent;
                current.ActivationOperator = currentServer == "/activesms" ? service.ActivationOperator : service.Operator;
                current.RequestedAt = currentServer == "/activesms" ? DateTime.Parse(service.ActivationTime) : DateTime.Parse(service.Expires);

                await _cacheService.SetMinutes($"{Strings.CURRENT_USER_RENT_SERVICE}:{ativationId}", JsonConvert.SerializeObject(current), 60);
            }

            var optionsCancelRent = InlineKeyboardButton.WithCallbackData($"❌ {_localizer["Cancel"]}", callbackData: $"/cancelrent:{ativationId}");

            var inlinekeyboard = new InlineKeyboardMarkup(new[] { optionsCancelRent });

            int messageId = await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, inlinekeyboard);
            if (currentServer == "/activesms")
            {
                //RecurringJob.AddOrUpdate($"{_chatId}:{messageId}", () => SchreduleDeleteMessage(_chatId, messageId), "*/20 * * * *");
            }
            else
            {
                RecurringJob.AddOrUpdate($"{_chatId}:{ativationId}", () => CheckSmsFiveSms(_chatId, ativationId), "*/6 * * * * *");
            }

            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);

            await RegisterData(messageId);

            await _cacheService.Delete($"{Strings.PRE_RENT_DATA_SMS_SERVICE}{_chatId}");
        }

        protected async Task OnListCoutries()
        {
            List<InlineKeyboardButton[][]> buttonArraysChunks = new();
            List<InlineKeyboardButton> coutriesBtn = new List<InlineKeyboardButton>();
            List<InlineKeyboardButton[]> buttonArrays = new List<InlineKeyboardButton[]>();

            foreach (var country in Strings.Countries)
            {
                coutriesBtn.Add(InlineKeyboardButton.WithCallbackData(text: Functions.EscapeMarkdownV2($"{country.Title}"), callbackData: $"changecountry:{country.Title}:{country.CountryNumber}"));

                if (coutriesBtn.Count == 2)
                {
                    buttonArrays.Add(coutriesBtn.ToArray());
                    coutriesBtn.Clear();
                }
            }

            if (coutriesBtn.Count > 0)
                buttonArrays.Add(coutriesBtn.ToArray());

            // Divida os botões em grupos de 20 usando LINQ
            buttonArraysChunks = buttonArrays
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / 4)
                .Select(group => group.Select(x => x.value).ToArray())
                .ToList();

            string caption =
                 $"🥷 {_localizer["Click on one of the countries below to change the service country"]}\n\n" +
                 $"📍 {_localizer["Country"]} {_localizer["Current"]}: {_selectedCountry}\n\n" +
                 $"👇 {_localizer["Options Below"]}";


            await _cacheService.Set($"{_chatId}{Strings.SERVICES_PAGINATION}", JsonConvert.SerializeObject(buttonArraysChunks), 1);
            await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_PAGE}", "0", 1);

            await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_SERVICES_PAGINATION}", "/paises", 1); // INDICA QUAL OBJETO ESTÁ NA PAGINACAO DO CACHE

            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
            var btsWithPagination = AddPaginationInKeyboardButtonTelegram(buttonArraysChunks[0].ToList());
            InlineKeyboardMarkup inlineKeyboardMarkup = new(btsWithPagination);

            await _telegramService.SendMedia(_chatId, "photo", $"{_configuration["Logo"]}", caption, inlineKeyboardMarkup);
        }

        protected async Task OnPreviousPageServices()
        {
            var inlinesjson = await _cacheService.Get($"{_chatId}{Strings.SERVICES_PAGINATION}");
            if (inlinesjson != null)
            {
                var inlines = JsonConvert.DeserializeObject<List<InlineKeyboardButton[][]>>(inlinesjson);
                int currentPage = int.Parse(await _cacheService.Get($"{_chatId}{Strings.CHATID_CURRENT_PAGE}"));
                if (inlines != null && currentPage > 0)
                {
                    int previousPage = currentPage - 1;

                    await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_PAGE}", $"{previousPage}", 1);
                    var btsWithPagination = AddPaginationInKeyboardButtonTelegram(inlines[previousPage].ToList());

                    InlineKeyboardMarkup inlineKeyboardMarkup = new(btsWithPagination);
                    await _telegramService.EditReplyMarkup(_chatId, _messageId, inlineKeyboardMarkup);
                }
            }
            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
        }

        protected async Task OnNextPageServices()
        {
            var inlinesjson = await _cacheService.Get($"{_chatId}{Strings.SERVICES_PAGINATION}");
            if (inlinesjson != null)
            {
                var inlines = JsonConvert.DeserializeObject<List<InlineKeyboardButton[][]>>(inlinesjson);
                int currentPage = int.Parse(await _cacheService.Get($"{_chatId}{Strings.CHATID_CURRENT_PAGE}"));
                if (inlines != null && currentPage < (inlines.Count - 1))
                {
                    int nextPage = currentPage + 1;

                    await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_PAGE}", $"{nextPage}", 1);
                    var btsWithPagination = AddPaginationInKeyboardButtonTelegram(inlines[nextPage].ToList());

                    InlineKeyboardMarkup inlineKeyboardMarkup = new(btsWithPagination);
                    await _telegramService.EditReplyMarkup(_chatId, _messageId, inlineKeyboardMarkup);
                }
            }
            await _telegramService.AnswerCallbackQueryAsync(messageId: _callbackMessageId);
        }

        protected async Task OnFirstPageServices()
        {
            var inlinesjson = await _cacheService.Get($"{_chatId}{Strings.SERVICES_PAGINATION}");
            if (inlinesjson != null)
            {
                var inlines = JsonConvert.DeserializeObject<List<InlineKeyboardButton[][]>>(inlinesjson);
                if (inlines != null)
                {
                    await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_PAGE}", $"{0}", 1);

                    var btsWithPagination = AddPaginationInKeyboardButtonTelegram(inlines[0].ToList());

                    InlineKeyboardMarkup inlineKeyboardMarkup = new(btsWithPagination);
                    await _telegramService.EditReplyMarkup(_chatId, _messageId, inlineKeyboardMarkup);
                }
            }
            _ = _telegramService.AnswerCallbackQueryAsync(messageId: _callbackMessageId);
        }

        protected async Task OnLastPageServices()
        {
            var inlinesjson = await _cacheService.Get($"{_chatId}{Strings.SERVICES_PAGINATION}");
            if (inlinesjson != null)
            {
                var inlines = JsonConvert.DeserializeObject<List<InlineKeyboardButton[][]>>(inlinesjson);
                if (inlines != null)
                {
                    await _cacheService.Set($"{_chatId}{Strings.CHATID_CURRENT_PAGE}", $"{inlines.Count - 1}", 1);

                    var btsWithPagination = AddPaginationInKeyboardButtonTelegram(inlines[^1].ToList());

                    InlineKeyboardMarkup inlineKeyboardMarkup = new(btsWithPagination);
                    await _telegramService.EditReplyMarkup(_chatId, _messageId, inlineKeyboardMarkup);
                }
            }
            _ = _telegramService.AnswerCallbackQueryAsync(messageId: _callbackMessageId);
        }

        protected async Task Cart(string input)
        {
            Cart cart = new();
            var cartcache = await _cacheService.Get($"{Strings.USER_CART}_{_chatId}");
            if (cartcache != null)
                cart = JsonConvert.DeserializeObject<Cart>(cartcache);
            else
                cart.ChatId = _chatId;

            await GetUserProfile();

            switch (input)
            {
                case "increment_1":
                    cart.CurrentValue += 1;
                    break;
                case "decrement_1":
                    cart.CurrentValue -= 1;
                    break;
                case "increment_5":
                    cart.CurrentValue += 5;
                    break;
                case "decrement_5":
                    cart.CurrentValue -= 5;
                    break;
                case "increment_20":
                    cart.CurrentValue += 20;
                    break;
                case "decrement_20":
                    cart.CurrentValue -= 20;
                    break;
                default:
                    break;
            }
            if (cart.CurrentValue < 5)
                cart.CurrentValue = 5;

            await _cacheService.Set($"{Strings.USER_CART}_{_chatId}", JsonConvert.SerializeObject(cart), 1);

            string caption =
                $"{Functions.EscapeMarkdownV2($"\U0001f977 {_firstName}!")} {_localizer["Choose how much you want to add to your balance"]}\n\n" +
                $"💰 {_localizer["Your balance"]}: {Functions.EscapeMarkdownV2(_userBalance)}\n\n" +
                $"💵 {_localizer["Value"]}: {cart.CurrentValue}";

            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
            _ = _telegramService.EditCaption(_chatId, _messageId, caption, CartActions());
        }

        protected InlineKeyboardMarkup CartActions()
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                // first row
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: $"-1", callbackData: "decrement_1"),
                    InlineKeyboardButton.WithCallbackData(text: $"+1", callbackData: "increment_1"),
                },
                // second row
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: $"-5", callbackData: "decrement_5"),
                    InlineKeyboardButton.WithCallbackData(text: $"+5", callbackData: "increment_5"),
                },
                // second row
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: $"-20", callbackData: "decrement_20"),
                    InlineKeyboardButton.WithCallbackData(text: $"+20", callbackData: "increment_20"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: $"💵 {_localizer["Purchase"]}", callbackData: "/purchase"),
                },
                new []
                {
                    optionStart,
                }
            });

            return inlineKeyboard;
        }

        protected List<InlineKeyboardButton[]> AddPaginationInKeyboardButtonTelegram(List<InlineKeyboardButton[]> inlines)
        {
            inlines.Insert(0, new InlineKeyboardButton[] { optionAddBalance });
            inlines.Insert(0, new InlineKeyboardButton[] { optionStart });
            inlines.Add(new InlineKeyboardButton[] { optionsBackButton, optionsNextButton });
            inlines.Add(new InlineKeyboardButton[] { optionsFirstButton, optionsLastButton });

            return inlines;
        }

        protected async Task SendMessageToChannel(string caption)
        {
            await _telegramService.SendText(_configuration["Telegram:GroupId"], caption);
        }

        public async Task ConfirmPix(long paymentId)
        {
            var session = await _cacheService.Get($"{paymentId}");
            SessionPaymentCache data = JsonConvert.DeserializeObject<SessionPaymentCache>(session);

            var payment = await _paymentService.GetPaymentById(paymentId);

            if (payment != null && payment.Status == "approved")
            {
                var userbalancerepository = await _userService.GetUserBalanceByChat(data.UserId);
                userbalancerepository ??= new();
                userbalancerepository.Balance += data.Quantity;
                userbalancerepository.Updated = DateTime.Now;

                if (userbalancerepository.Guid == string.Empty)
                    userbalancerepository.ChatId = data.UserId;

                UserBalanceHistory history = new()
                {
                    ChatId = data.UserId,
                    Amount = data.Quantity,
                    ConfirmedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    Status = payment.Status,
                    TransationType = TransationType.PIX,
                    TransactionId = payment.TransactionDetails.TransactionId
                };

                await _userService.RecordBalance(userbalancerepository);
                await _userService.RecordBalanceHistory(history);

                await _cacheService.Delete($"{paymentId}");
                await _cacheService.Delete($"{Strings.USER_CART}_{_chatId}");

                var user = await _userService.GetUserByChat(data.UserId);
                await GetUserProfile(data.UserId);

                string caption =
                    $"✅ {Functions.EscapeMarkdownV2($"{user.Username}!")} {_localizer["Balance added successfully"]}\n\n" +
                    $"💰 {_localizer["Your balance"]}: {Functions.EscapeMarkdownV2(_userBalance, code: true)}";

                InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    new []
                    {
                        optionStart
                    }
                });

                await _telegramService.SendMedia(data.UserId, "photo", $"{_configuration["Logo"]}", caption, inlineKeyboard);
                await _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
            }
        }

        public async Task OnSmsActiveReceiveSms(WKSms wkSms)
        {
            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);

            var rentcache = await _cacheService.Get($"{Strings.CURRENT_USER_RENT_SERVICE}:{wkSms.ActivationId}");
            if (rentcache == null) return;

            var rent = JsonConvert.DeserializeObject<CurrentSmsUserRentService>(rentcache);
            if (rent == null) return;

            var userbalance = await _userService.GetUserBalanceByChat(rent.ChatId);
            userbalance.Balance -= rent.ValueRent;
            userbalance.Updated = DateTime.Now;
            await _userService.RecordBalance(userbalance);

            RecurringJob.RemoveIfExists($"{rent.ChatId}:{rent.MessageId}");

            string caption =
                    $"\U0001f977 {_localizer["Your SMS code has arrived"]}\n" +
                    $"📄 {_localizer["Click on it to copy"]}\n\n" +
                    $"📥 {_localizer["Message"]}\n\n" +
                    $"🗒 {Functions.EscapeMarkdownV2(wkSms.Text, code: true)}\n\n" +
                    $"📱 {Functions.EscapeMarkdownV2(wkSms.Code, code: true)}\n";

            var inlinekeyboard = new InlineKeyboardMarkup(new[] { optionStart });
            _ = _telegramService.EditCaption(rent.ChatId, rent.MessageId, caption, inlinekeyboard);

            await _cacheService.Delete($"{Strings.CURRENT_USER_RENT_SERVICE}:{wkSms.ActivationId}");
        }

        public async Task CheckSmsFiveSms(string chatId, string orderId)
        {
            async Task RegisterData(CurrentSmsUserRentService current)
            {
                /*var userbalance = await _userService.GetUserBalanceByChat(chatId);

                userbalance.Balance += current.ValueRent;
                userbalance.Updated = DateTime.Now;
                await _userService.RecordBalance(userbalance);*/

                var history = await _userService.GetBalanceHistoryByTransactionId(current.ActivationId);
                history.Status = "canceled";
                history.CreatedAt = DateTime.Now;
                await _userService.RecordBalanceHistory(history);
            }

            var rentcache = await _cacheService.Get($"{Strings.CURRENT_USER_RENT_SERVICE}:{orderId}");
            if(rentcache == null)
            {
                RecurringJob.RemoveIfExists($"{chatId}:{orderId}");
                return;
            }
            var rent = JsonConvert.DeserializeObject<CurrentSmsUserRentService>(rentcache);

            _ = _telegramService.AnswerCallbackQueryAsync(_callbackMessageId);
            var sms = await _fivesimService.CheckOrder(orderId);
            if (sms != null && (sms.Status == "CANCELED"))
            {
                RecurringJob.RemoveIfExists($"{chatId}:{orderId}");

                if (rent != null)
                    await RegisterData(rent);

                return;
            }

            if (sms != null && (sms.Status == "FINISHED" || sms.Status == "BANNED"))
            {
                RecurringJob.RemoveIfExists($"{chatId}:{orderId}");
                return;
            }

            if (sms != null && sms.Status == "TIMEOUT")
            {
                RecurringJob.RemoveIfExists($"{chatId}:{orderId}");

                if(rent != null)
                    await RegisterData(rent);

                string caption =
                    $"❌ {_localizer["Expired time"]}\n" +
                    $"\U0001f977 {_localizer["Select another number to receive the SMS"]}\n";

                var inlinekeyboard = new InlineKeyboardMarkup(new[] { optionStart });
                _ = _telegramService.EditCaption(chatId, rent.MessageId, caption, inlinekeyboard);

                await _cacheService.Delete($"{Strings.CURRENT_USER_RENT_SERVICE}:{orderId}");
                return;
            }

            if (sms != null && sms.Sms != null && sms.Sms.Count > 0)
            {
                RecurringJob.RemoveIfExists($"{chatId}:{orderId}");

                rentcache = await _cacheService.Get($"{Strings.CURRENT_USER_RENT_SERVICE}:{orderId}");
                if (rentcache == null)
                {
                    _ = _fivesimService.CancelOrder(orderId);
                    return;
                }

                rent = JsonConvert.DeserializeObject<CurrentSmsUserRentService>(rentcache);
                if (rent == null)
                {
                    _ = _fivesimService.CancelOrder(orderId);
                    return;
                }

                var userbalance = await _userService.GetUserBalanceByChat(rent.ChatId);
                userbalance.Balance -= rent.ValueRent;
                userbalance.Updated = DateTime.Now;
                await _userService.RecordBalance(userbalance);

                string caption =
                    $"\U0001f977 {_localizer["Your SMS code has arrived"]}\n" +
                    $"📄 {_localizer["Click on it to copy"]}\n\n" +
                    $"📥 {_localizer["Message"]}\n\n" +
                    $"🗒 {Functions.EscapeMarkdownV2(sms.Sms.First().Text, code: true)}\n\n" +
                    $"📱 {Functions.EscapeMarkdownV2(sms.Sms.First().Code, code: true)}\n";

                var inlinekeyboard = new InlineKeyboardMarkup(new[] { optionStart });
                _ = _telegramService.EditCaption(chatId, rent.MessageId, caption, inlinekeyboard);

                _ = _cacheService.Delete($"{Strings.CURRENT_USER_RENT_SERVICE}:{orderId}");
            }
        }

        public async Task SchreduleDeleteMessage(string chatId, int messageId)
        {
            RecurringJob.RemoveIfExists($"{chatId}:{messageId}");

            var inlinekeyboard = new InlineKeyboardMarkup(new[] { optionStart });
            string caption =
                    $"⛔️ {_localizer["Expired time"]}";

            _ = _telegramService.EditCaption(chatId, messageId, caption, inlinekeyboard);
        }

        public async Task SchreduleCancelAtivation(string chatId, string ativationId)
        {
            var cancelSms = await _activeSmsService.UpdateStatusAtivationService(ativationId, Domain.ActiveSms.StatusActivation.Cancel);
            var ativationcache = await _cacheService.Get($"{Strings.CURRENT_USER_RENT_SERVICE}:{ativationId}");
            if (ativationcache != null)
            {
                //var ativation = JsonConvert.DeserializeObject<CurrentSmsUserRentService>(ativationcache);
                if (!cancelSms)
                    RecurringJob.AddOrUpdate(ativationId, () => SchreduleCancelAtivation(chatId, ativationId), "*/1 * * * *");
                else
                {
                    RecurringJob.RemoveIfExists(ativationId);
                    await _cacheService.Delete($"{Strings.CURRENT_USER_RENT_SERVICE}:{ativationId}");
                }
            }
            else
                RecurringJob.RemoveIfExists(ativationId);
        }

        public async Task GenerateVoucher(IStringLocalizer<object> localizer)
        {
            var newLocalizer = localizer as IStringLocalizer<BotSmsService>;

            Voucher voucher = new();
            voucher.Quantity = 1;
            voucher.CreatedAt = DateTime.Now;
            voucher.Guid = Guid.NewGuid().ToString();

            Random random = new();
            double minValue = 0.10;
            double maxValue = 0.50;

            double randomValue = Math.Round(random.NextDouble() * (maxValue - minValue) + minValue, 2);
            voucher.Amount = randomValue;
            voucher.Code = $"/NBV{randomValue * 100}";

            await _voucherRepository.Record(voucher);

            var c = new CultureInfo("pt-br");
            string profitFormated = voucher.Amount.ToString("C2", c);

            string caption = $"🔖 **{newLocalizer["VOUCHER OF THE DAY"]}**\n\n" +
                $"ℹ️ {newLocalizer["Use the code"]} **{Functions.EscapeMarkdownV2(voucher.Code, code: true)}** {newLocalizer["to receive credits from"]} {profitFormated}\n\n" + 
                $"🥷 {newLocalizer["Only"]} {voucher.Quantity} {newLocalizer["available units"]}";

            await SendMessageToChannel(caption);
        }

        public async Task<Voucher> VoucherToday() => await _voucherRepository.GetByDate(DateTime.Now);

        protected double MakeFixedPrice(string serviceKey, double currentPrice)
        {
            switch (serviceKey)
            {
                case "auc":
                    return 1.00;
                case "wa":
                    return 15;
                case "tg":
                    return 7.50;
                default:
                    return currentPrice;
            }
        }
    }
}
