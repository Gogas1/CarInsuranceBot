using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Actions.Abstractions;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions
{
    /// <summary>
    /// <see cref="CallbackQueryActionBase"/> implementation to process user reconsideration to continue ordering an insurance. 
    /// <para>User state on exit - <see cref="UserState.Home"/></para>
    /// </summary>
    internal class ProcessDocumentsSubmittingCallbackQueryAction : CallbackQueryActionBase
    {
        private readonly OpenAIService _openAiService;

        public ProcessDocumentsSubmittingCallbackQueryAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAiService) : base(userService, botClient)
        {
            _openAiService = openAiService;
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            //await _botClient.AnswerCallbackQuery(update.Id, cancellationToken: cancellationToken);

            //Send message about reconsideration with home keyboard markup
            if(update.Data == AnswersData.AUTHORIZATION_DECLINE_BUTTON_DATA)
            {
                await _botClient.SendMessage(
                    update.From.Id,
                    AnswersData.USER_RECONSIDERED_ANSWER_TEXT,
                    replyMarkup: AnswersData.HOME_KEYBOARD,
                    cancellationToken: cancellationToken);

                //Change user state to UserState.Home
                await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id, cancellationToken: cancellationToken);
            }

            if(update.Data == AnswersData.SHARE_DOCUMENTS_IN_CHAT_BUTTON_DATA)
            {
                if (update.From == null)
                {
                    return;
                }

                await _botClient.SendMessage(update.From.Id, await _openAiService.GetDiversifiedAnswer(AnswersData.SHARE_PASSPORT_IN_CHAT_GPT_SETTINGS, cancellationToken));
                await _userService.SetUserStateByTelegramIdAsync(UserState.PassportAwait, update.From.Id, cancellationToken);
            }
        }
    }
}
