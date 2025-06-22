using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.PriceConfirmationAwait
{
    /// <summary>
    /// <see cref="CallbackQueryActionBase"/> implementation action to process user user agreement or disagreement with price.
    /// <para>Exit states: 
    /// <see cref="Enums.UserState.DocumentsAwait"/> if user's documents data no longer stored, 
    /// <see cref="Enums.UserState.Home"/> if agree, 
    /// <see cref="Enums.UserState.PriceConfirmationAwait"/> if disagree</para>
    /// </summary>
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
            //await _botClient.AnswerCallbackQuery(update.Id, cancellationToken: cancellationToken);

            var data = update.Data;
            if (data == null)
            {
                return;
            }

            //If user agree
            if (data == AnswersData.DATA_CONFIRMATION_BUTTON_DATA)
            {
                await ProcessAgreement(update, cancellationToken);
                return;
            }

            //If user disagree
            if (data == AnswersData.DATA_DECLINE_BUTTON_DATA)
            {
                await ProcessDecline(update, cancellationToken);
                return;
            }
        }

        /// <summary>
        /// Process user agreement with price
        /// </summary>
        /// <param name="update"><see cref="CallbackQuery"/> instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="Task"/></returns>
        protected virtual async Task ProcessAgreement(CallbackQuery update, CancellationToken cancellationToken)
        {
            // Create insurance for user
            var insuranceDocumentData = await _insuranceService.CreateInsuranceForUser(update.From.Id, cancellationToken);

            // If no data - creating failure
            if (insuranceDocumentData == null)
            {
                // Create and set new nonce
                var newNonce = _documentsService.SetNonceForUser(update.From.Id);

                //Send message about failure with the authorization keyboard
                await _botClient.SendMessage(
                    update.From.Id,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_STORED_DOCUMENTS_SETTINGS, cancellationToken),
                    replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfig, newNonce),
                    cancellationToken: cancellationToken);
                
                // Change state
                await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, update.From.Id, cancellationToken);
                return;
            }

            // New memory stream to handle PDF fiel creation and sending
            using var stream = new MemoryStream();
            
            // Generate PDF file
            _pdfService.GenerateInsurancePdf(insuranceDocumentData, stream);
            
            // Reset stream
            stream.Position = 0;

            // Instantiate telegram file to handle file info
            var file = new InputFileStream(stream, "INSURANCE POLICY.pdf");

            // Change state
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.Home, update.From.Id, cancellationToken);

            // Send PDF file
            await _botClient.SendDocument(update.From.Id, file, cancellationToken: cancellationToken);

            // Send message about sucess
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.INSURANCE_GRANTED_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Process user disagreement with price
        /// </summary>
        /// <param name="update"><see cref="CallbackQuery"/> instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="Task"/></returns>
        protected virtual async Task ProcessDecline(CallbackQuery update, CancellationToken cancellationToken)
        {
            // Send message about only one price availability
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.FIRST_PRICE_DECLINE_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.PRICE_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);

            // Change state
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PriceSecondConfirmationAwait, update.From.Id, cancellationToken);
        }
    }
}
