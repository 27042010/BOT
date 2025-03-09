using Domain;
using Domain.Interfaces.Repository;

namespace Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserBalanceRepository _userBalanceRepository;
        private readonly IUserBalanceHistoryRepository _userbalancehistoryrepository;
        private readonly IUserBlackListRepository _userBlackListRepository;

        public UserService(
            IUserRepository userRepository, 
            IUserBalanceRepository userBalanceRepository,
            IUserBalanceHistoryRepository userbalancehistoryrepository,
            IUserBlackListRepository userBlackListRepository)
        {
            _userRepository = userRepository;
            _userBalanceRepository = userBalanceRepository;
            _userbalancehistoryrepository = userbalancehistoryrepository;
            _userBlackListRepository = userBlackListRepository;
        }

        public async Task BanUser(string chatId, BlackListReason reason)
        {
            UserBlackList blackList = new();
            blackList.ChatId = chatId;
            blackList.Reason = reason;
            blackList.CreatedAt = DateTime.Now;

            await _userBlackListRepository.Record(blackList);
        }

        public async Task<UserBlackList> GetBan(string chatId) => await _userBlackListRepository.GetByChatId(chatId);

        public async Task UnBanUser(string chatId) => await _userBlackListRepository.DeleteByChatId(chatId);

        public async Task DeleteBalanceHistoryByGuid(string guid) => await _userbalancehistoryrepository.Delete(guid);

        public async Task<UserBalanceHistory> GetBalanceHistoryByTransactionId(string transactionId) => await _userbalancehistoryrepository.GetBalanceHistoryByTransactionId(transactionId);

        public async Task<UserBalance> GetUserBalanceByChat(string chatId) => await _userBalanceRepository.GetByChatId(chatId);

        public async Task<User> GetUserByChat(string chatId) => await _userRepository.GetByChatId(chatId);

        public async Task Record(User user) => await _userRepository.Record(user);

        public async Task RecordBalance(UserBalance history)
        {
            if (history.Guid == string.Empty)
            {
                history.Guid = Guid.NewGuid().ToString();
                history.Created = DateTime.Now;
                await _userBalanceRepository.Record(history);
            }
            else
                await _userBalanceRepository.Update(history);
        }

        public async Task<string> RecordBalanceHistory(UserBalanceHistory history)
        {
            try
            {
                if (history.Guid == string.Empty)
                {
                    history.Guid = Guid.NewGuid().ToString();
                    await _userbalancehistoryrepository.Record(history);
                }
                else
                    await _userbalancehistoryrepository.Update(history);

                return history.Guid;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task Update(User user)
        {
            var userrepository = await _userRepository.GetByChatId(user.ChatId);
            if (userrepository == null)
            {
                await _userRepository.Record(user);
                return;
            }

            userrepository.ChatId = user.ChatId;
            userrepository.Username = user.Username;
            userrepository.ContactId = user.ContactId;
            userrepository.Language = user.Language;
            userrepository.SelectetdCountryCode = user.SelectetdCountryCode;
            userrepository.SelectetdCountryName = user.SelectetdCountryName;
            userrepository.UpdatedAt = DateTime.Now;

            await _userRepository.Update(userrepository);
        }

        public async Task<List<UserBalanceHistory>> GetBalanceHistoryByChatIdAndStatus(string chatId, string status, TransationType transationType)
        {
            return await _userbalancehistoryrepository.GetBalanceHistoryByChatIdAndStatus(chatId, status, transationType);
        }

        public async Task<List<UserBalanceHistory>> GetBalanceHistoryByChatIdAndStatus(string chatId, TransationType transationType)
        {
            return await _userbalancehistoryrepository.GetBalanceHistoryByChatIdAndStatus(chatId, transationType);
        }
    }
}
