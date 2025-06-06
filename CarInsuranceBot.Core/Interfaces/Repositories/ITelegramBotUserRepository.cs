using CarInsuranceBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Interfaces.Repositories
{
    public interface ITelegramBotUserRepository : ICrudRepository<MyUser, Guid>
    {
        public Task<MyUser?> GetUserByTelegramIdAsync(long telegramId);
        public Task<MyUser?> GetUserWithInputStateByTelegramIdAsync(long telegramId);
        public Task<UserInputState?> GetUserInputStateByTelegramIdAsync(long telegramId);
    }
}
