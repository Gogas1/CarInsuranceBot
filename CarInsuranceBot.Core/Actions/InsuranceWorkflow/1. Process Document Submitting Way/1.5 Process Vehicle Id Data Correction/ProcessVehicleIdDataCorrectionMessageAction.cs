using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Actions.InsuranceWorkflow;
using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Models.Documents;
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

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    internal class ProcessVehicleIdDataCorrectionMessageAction : DocumentCorrectionBaseAction<DriverLicenseDocument>
    {
        private static readonly CompositeFormat _vehicleIdDataFormat =
       CompositeFormat.Parse(AnswersData.VEHICLE_ID_DATA_TEMPLATE_TEXT);

        public ProcessVehicleIdDataCorrectionMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            DocumentsService documentsService,
            OpenAIService openAiService,
            IOptions<BotConfiguration> botConfig,
            SecretCache secretCache) : base(userService, botClient, documentsService, openAiService, botConfig, secretCache)
        {
        }

        protected override List<DocumentFieldModel<DriverLicenseDocument>> GetInvalidations(DriverLicenseDocument document)
        {
            return document.GetInvalidFieldsHandlers();
        }

        protected override async Task OnNoDocuments(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

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
        }

        protected override async Task OnNoInvalidations(Message update, DriverLicenseDocument document, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            var message = string.Format(
                    CultureInfo.InvariantCulture,
                    _vehicleIdDataFormat,
                    document.RegistrationNumber,
                    document.RegistrationDate.ToString("yyyy.MM.dd")
                );

            await _botClient.SendMessage(
                update.From.Id,
                message,
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.LicenseDataConfirmationAwait, update.From.Id, cancellationToken);
        }

        protected override async Task<DriverLicenseDocument?> RetreiveDocument(Message update, CancellationToken cancellationToken)
        {
            if(update.From == null)
            {
                return null;
            }

            var data = await _documentsService.GetDataForUserAsync(update.From.Id, cancellationToken);

            return data?.DriverLicenseDocument ?? null;
        }
    }
}
