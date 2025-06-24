using CarInsuranceBot.Core.Actions.InsuranceWorkflow;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions
{
    internal class ProcessVehicleIdDataConfirmationCallbackAction : ProcessConfirmationBaseCallbackQueryAction
    {
        private readonly DocumentsService _documentsService;
        private readonly BotConfiguration _botConfig;
        private readonly OpenAIService _openAiService;

        private static readonly CompositeFormat _documentsProvidedFormat =
        CompositeFormat.Parse(AnswersData.DOCUMENTS_PROVIDED_TEXT);

        public ProcessVehicleIdDataConfirmationCallbackAction(
            UserService userService,
            ITelegramBotClient botClient,
            DocumentsService documentsService,
            IOptions<BotConfiguration> botConfig,
            OpenAIService openAiService) : base(userService, botClient)
        {
            _documentsService = documentsService;
            _botConfig = botConfig.Value;
            _openAiService = openAiService;
        }

        protected override async Task OnConfirmation(CallbackQuery update, CancellationToken cancellationToken)
        {
            var data = await _documentsService.GetDataForUserAsync(update.From.Id, cancellationToken);

            if (data == null)
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

            var message = string.Format(
                CultureInfo.InvariantCulture,
                _documentsProvidedFormat,
                data.idDocument.DocumentNumber,
                data.idDocument.CountryCode,
                data.idDocument.Surname,
                data.idDocument.Name,
                data.idDocument.BirthDate.ToString("yyyy.MM.dd"),
                data.idDocument.ExpiryDate.ToString("yyyy.MM.dd"),

                data.DriverLicenseDocument.RegistrationNumber,
                data.DriverLicenseDocument.RegistrationDate.ToString("yyyy.MM.dd")
            );

            await _botClient.SendMessage(
                update.From.Id,
                message,
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsDataConfirmationAwait, update.From.Id, cancellationToken);
        }

        protected override async Task OnDecline(CallbackQuery update, CancellationToken cancellationToken)
        {
            if (update.From == null) { return; }

            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.DATA_DECLINED_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD,
                cancellationToken: cancellationToken);

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.LicenseAwait, update.From.Id, cancellationToken: cancellationToken);
        }
    }
}
