using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Interfaces.Repositories;
using CarInsuranceBot.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CarInsuranceBot.Core.Services
{
    /// <summary>
    /// Service to handle users
    /// </summary>
    internal class UserService
    {
        private readonly ITelegramBotUserRepository _userRepository;
        private readonly MemoryCache _memoryCache;

        public UserService(ITelegramBotUserRepository userRepository, MemoryCache memoryCache)
        {
            _userRepository = userRepository;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Sets bot busyness flag for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isBusy"></param>
        public void SetUserBusy(long userId, bool isBusy)
        {
            _memoryCache.Set($"busy_{userId}", isBusy, TimeSpan.FromSeconds(60));
        }


        /// <summary>
        /// Gets bot busyness flag for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsUserBusy(long userId)
        {
            return _memoryCache.Get<bool>($"busy_{userId}");
        }

        /// <summary>
        /// Gets user state by their telegram id
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<UserState> GetUserStateByTelegramIdAsync(long telegramId, CancellationToken cancellationToken)
        {
            var targetUser = await _userRepository.GetUserByTelegramIdAsync(telegramId, cancellationToken);

            if (targetUser == null)
            {
                targetUser = new Models.MyUser()
                {
                    TelegramId = telegramId,
                    UserState = UserState.None,
                };

                await _userRepository.CreateAsync(targetUser, cancellationToken);
            }

            return targetUser.UserState;
        }

        /// <summary>
        /// Sets user state by their telegram id
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetUserStateByTelegramIdAsync(UserState newState, long telegramId, CancellationToken cancellationToken)
        {
            var targetUser = await _userRepository.GetUserByTelegramIdAsync(telegramId, cancellationToken);

            if (targetUser == null)
            {
                targetUser = new MyUser()
                {
                    TelegramId = telegramId,
                    UserState = newState,
                };

                await _userRepository.CreateAsync(targetUser, cancellationToken);
                return;
            }

            if (targetUser.UserState == newState)
            {
                return;
            }

            targetUser.UserState = newState;
            await _userRepository.UpdateAsync(targetUser, cancellationToken);
        }

        /// <summary>
        /// Sets user input state by their telegram id
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="handler">User current input state handling function</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetUserInputStateAsync(long telegramId, Action<UserInputState> handler, CancellationToken cancellationToken)
        {
            var targetUser = await _userRepository.GetUserWithInputStateByTelegramIdAsync(telegramId, cancellationToken);
            if (targetUser == null)
            {
                targetUser = new MyUser
                {
                    TelegramId = telegramId,
                    InputState = new UserInputState()
                };
                await _userRepository.CreateAsync(targetUser, cancellationToken);
                return;
            }

            targetUser.InputState ??= new UserInputState();
            handler(targetUser.InputState);
            await _userRepository.UpdateAsync(targetUser, cancellationToken);
        }

        /// <summary>
        /// Gets user input state by their telegram id
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<UserInputState> GetUserInputStateAsync(long telegramId, CancellationToken cancellationToken)
        {
            var inputState = await _userRepository.GetUserInputStateByTelegramIdAsync(telegramId, cancellationToken);

            if (inputState == null)
            {
                return new UserInputState();
            }

            return inputState;
        }
    }
}
