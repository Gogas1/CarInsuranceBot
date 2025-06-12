using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Interfaces.Repositories;
using CarInsuranceBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Services
{
    internal class UserService
    {
        private readonly ITelegramBotUserRepository _userRepository;

        public UserService(ITelegramBotUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserState> GetUserStateByTelegramIdAsync(long telegramId)
        {
            var targetUser = await _userRepository.GetUserByTelegramIdAsync(telegramId);

            if(targetUser == null)
            {
                targetUser = new Models.MyUser()
                {
                    TelegramId = telegramId,
                    UserState = UserState.None,
                };

                await _userRepository.CreateAsync(targetUser);
            }

            return targetUser.UserState;
        }

        public async Task SetUserStateByTelegramIdAsync(UserState newState, long telegramId)
        {
            var targetUser = await _userRepository.GetUserByTelegramIdAsync(telegramId);

            if (targetUser == null)
            {
                targetUser = new MyUser()
                {
                    TelegramId = telegramId,
                    UserState = newState,
                };

                await _userRepository.CreateAsync(targetUser);
                return;
            }

            if (targetUser.UserState == newState)
            {
                return;
            }

            targetUser.UserState = newState;
            await _userRepository.UpdateAsync(targetUser);
        }

        public async Task SetUserInputStateAsync(long telegramId, Action<UserInputState> handler)
        {
            var targetUser = await _userRepository.GetUserWithInputStateByTelegramIdAsync(telegramId);
            if (targetUser == null)
            {
                targetUser = new MyUser
                {
                    TelegramId = telegramId,
                    InputState = new UserInputState()
                };
                await _userRepository.CreateAsync(targetUser);
                return;
            }

            targetUser.InputState ??= new UserInputState();
            handler(targetUser.InputState);
            await _userRepository.UpdateAsync(targetUser);
        }

        public async Task<UserInputState> GetUserInputStateAsync(long telegramId)
        {
            var inputState = await _userRepository.GetUserInputStateByTelegramIdAsync(telegramId);

            if(inputState == null)
            {
                return new UserInputState();
            }

            return inputState;
        }
    }
}
