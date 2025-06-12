using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Services
{
    public class TestService
    {
        private readonly UserService _userService;

        public TestService(IServiceProvider serviceProvider) 
        {
            _userService = serviceProvider.GetRequiredService<UserService>();
        }

        public async Task ToTestState(long userId)
        {
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.TestUserState, userId);
        }
    }
}
