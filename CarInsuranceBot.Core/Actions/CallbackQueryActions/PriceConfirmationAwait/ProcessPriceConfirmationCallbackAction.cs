using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.PriceConfirmationAwait
{
    internal class ProcessPriceConfirmationCallbackAction : CallbackQueryActionBase
    {
        private readonly BotConfiguration _botConfig;
        private readonly InsuranceService _insuranceService;
        private readonly PdfService _pdfService;
        protected readonly OpenAIService _openAiService;
        private readonly DocumentsService _documentsService;

        public ProcessPriceConfirmationCallbackAction(
            UserService userService,
            ITelegramBotClient botClient,
            IOptions<BotConfiguration> botOptions,
            InsuranceService insuranceService,
            PdfService pdfService,
            OpenAIService openAIService,
            DocumentsService documentsService) : base(userService, botClient)
        {
            _botConfig = botOptions.Value;
            _insuranceService = insuranceService;
            _pdfService = pdfService;
            _openAiService = openAIService;
            _documentsService = documentsService;
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            await _botClient.AnswerCallbackQuery(update.Id, cancellationToken: cancellationToken);

            var data = update.Data;
            if (data == null)
            {
                return;
            }

            if (data == AnswersData.DATA_CONFIRMATION_BUTTON_DATA)
            {
                await ProcessAgreement(update, cancellationToken);
                return;
            }

            if (data == AnswersData.DATA_DECLINE_BUTTON_DATA)
            {
                await ProcessDecline(update, cancellationToken);
                return;
            }
        }

        protected virtual async Task ProcessAgreement(CallbackQuery update, CancellationToken cancellationToken)
        {
            var insuranceDocumentData = await _insuranceService.CreateInsuranceForUser(update.From.Id, cancellationToken);

            if (insuranceDocumentData == null)
            {
                var newNonce = _documentsService.SetNonceForUser(update.From.Id);

                await _botClient.SendMessage(
                    update.From.Id,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_STORED_DOCUMENTS_SETTINGS, cancellationToken),
                    replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfig, newNonce),
                    cancellationToken: cancellationToken);
                await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, update.From.Id, cancellationToken);
                return;
            }

            using var stream = new MemoryStream();
            _pdfService.GenerateInsurancePdf(insuranceDocumentData, stream);
            stream.Position = 0;
            var file = new InputFileStream(stream, "INSURANCE POLICY");
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.Home, update.From.Id, cancellationToken);
            await _botClient.SendDocument(update.From.Id, file, cancellationToken: cancellationToken);
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.INSURANCE_GRANTED_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);
        }

        protected virtual async Task ProcessDecline(CallbackQuery update, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.FIRST_PRICE_DECLINE_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.PRICE_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PriceSecondConfirmationAwait, update.From.Id, cancellationToken);
        }


    }
}
