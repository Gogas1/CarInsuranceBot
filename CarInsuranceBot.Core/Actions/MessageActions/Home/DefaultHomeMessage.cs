using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Actions.MessageActions.Home
{
    internal class DefaultHomeMessage : MessageActionBase
    {
        public DefaultHomeMessage(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        public override async Task Execute(Message update)
        {
            await base.Execute(update);

            if (update.From == null)
            {
                return;
            }

            await _botClient.SendMessage(update.Chat, AnswersData.DefaultHomeTextFallback, replyMarkup: AnswersData.GetInsuranceInlineButton);
            await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id);
        }
    }
}
