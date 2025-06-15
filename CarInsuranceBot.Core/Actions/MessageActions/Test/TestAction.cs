using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions.Test
{
    internal class TestAction : MessageActionBase
    {
        

        public TestAction(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        protected override async Task ProcessLogicAsync(Message update, CancellationToken timeoutToken)
        {
            if (update.From == null)
            {
                return;
            }

            await _botClient.SendMessage(update.From.Id, "Start test action", cancellationToken: timeoutToken);
            await Task.Delay(TimeSpan.FromSeconds(30), timeoutToken);
            await _botClient.SendMessage(update.From.Id, "End test action", cancellationToken: timeoutToken);
        }
    }
}
