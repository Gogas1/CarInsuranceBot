using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using CarInsuranceBot.Core.Enums;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.DocumentsAwait
{
    /// <summary>
    /// <see cref="CallbackQueryActionBase"/> implementation to process user reconsideration to continue ordering an insurance. 
    /// <para>User state on exit - <see cref="UserState.Home"/></para>
    /// </summary>
    internal class ProcessReconsiderationAction : CallbackQueryActionBase
    {
        public ProcessReconsiderationAction(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            //await _botClient.AnswerCallbackQuery(update.Id, cancellationToken: cancellationToken);

            //Send message about reconsideration with home keyboard markup
            await _botClient.SendMessage(
                update.From.Id,
                AnswersData.USER_RECONSIDERED_ANSWER_TEXT,
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);

            //Change user state to UserState.Home
            await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id, cancellationToken: cancellationToken);
        }
    }
}
