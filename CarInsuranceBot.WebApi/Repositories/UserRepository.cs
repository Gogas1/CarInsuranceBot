using CarInsuranceBot.Core.Models;
using CarInsuranceBot.WebApi.DbContexts;
using CarInsuranceBot.WebApi.Interfaces;

namespace CarInsuranceBot.WebApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CarInsuranceDbContext _context;

        public UserRepository(CarInsuranceDbContext context)
        {
            _context = context;
        }

        public Task<MyUser> CreateAsync(MyUser entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(MyUser entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<MyUser?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<MyUser?> GetUserByTelegramIdAsync(long telegramId)
        {
            throw new NotImplementedException();
        }

        public Task<UserInputState?> GetUserInputStateByTelegramIdAsync(long telegramId)
        {
            throw new NotImplementedException();
        }

        public Task<MyUser?> GetUserWithInputStateByTelegramIdAsync(long telegramId)
        {
            throw new NotImplementedException();
        }

        public Task<MyUser> UpdateAsync(MyUser entity)
        {
            throw new NotImplementedException();
        }
    }
}
