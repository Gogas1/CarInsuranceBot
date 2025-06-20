using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.PassportAwait
{
    internal class PassportAwaitCallbackAction : CallbackQueryActionBase
    {
        public PassportAwaitCallbackAction(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                update.From.Id,
                AnswersData.USER_RECONSIDERED_ANSWER_TEXT,
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);
            await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id, cancellationToken);
        }
    }
}
