using CarInsuranceBot.Core.Models;
using CarInsuranceBot.WebApi.DbContexts;
using CarInsuranceBot.WebApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarInsuranceBot.WebApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CarInsuranceDbContext _context;

        public UserRepository(CarInsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<MyUser> CreateAsync(MyUser entity, CancellationToken cancellationToken)
        {
            _context.Users.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task DeleteAsync(MyUser entity, CancellationToken cancellationToken)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            await _context.Users.Where(u => u.Id == id).ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<MyUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<MyUser?> GetUserByTelegramIdAsync(long telegramId, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);
        }

        public async Task<UserInputState?> GetUserInputStateByTelegramIdAsync(long telegramId, CancellationToken cancellationToken)
        {
            var targetUser = await _context.Users
                .Include(u => u.InputState)
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);
            if(targetUser == null)
            {
                return null;
            }

            return targetUser.InputState;
        }

        public async Task<MyUser?> GetUserWithInputStateByTelegramIdAsync(long telegramId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.InputState)
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);
        }

        public async Task<MyUser> UpdateAsync(MyUser entity, CancellationToken cancellationToken)
        {
            _context.Users.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
    }
}
