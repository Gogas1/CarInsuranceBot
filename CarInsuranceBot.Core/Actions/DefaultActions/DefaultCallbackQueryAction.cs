using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.DefaultActions
{
    internal class DefaultCallbackQueryAction : CallbackQueryActionBase
    {
        public DefaultCallbackQueryAction(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            if(update.Data == AnswersData.STOP_WORKFLOW_BUTTON_DATA)
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
}
