using CarInsuranceBot.Core.Models;

namespace CarInsuranceBot.Core.Interfaces.Repositories
{
    public interface ITelegramBotUserRepository : ICrudRepository<MyUser, Guid>
    {
        public Task<MyUser?> GetUserByTelegramIdAsync(long telegramId, CancellationToken cancellationToken);
        public Task<MyUser?> GetUserWithInputStateByTelegramIdAsync(long telegramId, CancellationToken cancellationToken);
        public Task<UserInputState?> GetUserInputStateByTelegramIdAsync(long telegramId, CancellationToken cancellationToken);
    }
}
