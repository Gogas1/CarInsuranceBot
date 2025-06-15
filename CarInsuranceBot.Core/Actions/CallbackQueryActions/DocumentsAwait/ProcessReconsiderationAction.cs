using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.DocumentsAwait
{
    internal class ProcessReconsiderationAction : CallbackQueryActionBase
    {
        public ProcessReconsiderationAction(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            await _botClient.AnswerCallbackQuery(update.Id, cancellationToken: cancellationToken);
            await _botClient.SendMessage(
                update.From.Id, 
                AnswersData.USER_RECONSIDERED_ANSWER_TEXT, 
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.Home, update.From.Id, cancellationToken: cancellationToken);
        }
    }
}
