using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.Home
{
    /// <summary>
    /// <see cref="CallbackQueryActionBase"/> implementation action to process insurance ordering start.
    /// <para>Exit state - <see cref="Enums.UserState.DocumentsAwait"/></para>
    /// </summary>
    internal class InitCreateInsuranceFlow : CallbackQueryActionBase
    {
        private readonly BotConfiguration _botConfig;
        private readonly OpenAIService _openAIService;
        private readonly DocumentsService _documentsService;

        public InitCreateInsuranceFlow(
            UserService userService,
            ITelegramBotClient botClient,
            IOptions<BotConfiguration> botOptions,
            OpenAIService openAIService,
            DocumentsService documentsService) : base(userService, botClient)
        {
            _botConfig = botOptions.Value;
            _openAIService = openAIService;
            _documentsService = documentsService;
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            //await _botClient.AnswerCallbackQuery(update.Id);

            // Create and set new nonce for user
            var nonce = _documentsService.SetNonceForUser(update.From.Id);

            // Send message about submitting the documents and authorization keyboard
            await _botClient.SendMessage(
                update.From.Id,
                await _openAIService.GetDiversifiedAnswer(AnswersData.START_INSURANCE_WORKFLOW_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfig, nonce));

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, update.From.Id, cancellationToken);
        }
    }
}
