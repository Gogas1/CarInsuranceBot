using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.DocumentsConfirmationAwait
{
    internal class ProcessDataConfirmationCallbackAction : CallbackQueryActionBase
    {

        private readonly BotConfiguration _botConfig;
        private readonly OpenAIService _openAIService;
        private readonly DocumentsService _documentsService;

        public ProcessDataConfirmationCallbackAction(
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
            await _botClient.AnswerCallbackQuery(update.Id);

            var data = update.Data;
            if (data == null)
            {
                return;
            }

            if (data == AnswersData.DATA_CONFIRMATION_BUTTON_DATA)
            {
                await _botClient.SendMessage(
                    update.From.Id,
                    await _openAIService.GetDiversifiedAnswer(AnswersData.DATA_CONFIRMED_SETTINGS, cancellationToken),
                    replyMarkup: AnswersData.PRICE_CONFIRMATION_KEYBOARD,
                    cancellationToken: cancellationToken);
                await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PriceConfirmationAwait, update.From.Id, cancellationToken);
                return;
            }

            if (data == AnswersData.DATA_DECLINE_BUTTON_DATA)
            {
                var newNonce = _documentsService.SetNonceForUser(update.From.Id);

                await _botClient.SendMessage(
                    update.From.Id,
                    await _openAIService.GetDiversifiedAnswer(AnswersData.DATA_DECLINED_SETTINGS, cancellationToken),
                    replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfig, newNonce));
                await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, update.From.Id, cancellationToken);
                return;
            }
        }
    }
}
