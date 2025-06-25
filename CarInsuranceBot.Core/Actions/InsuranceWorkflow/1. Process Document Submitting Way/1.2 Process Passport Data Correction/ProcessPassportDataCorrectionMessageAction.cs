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
using Telegram.Bot.Types.Passport;

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    internal class ProcessPassportDataCorrectionMessageAction : DocumentCorrectionBaseAction<IdDocument>
    {

        private static readonly CompositeFormat _passportDataFormat =
       CompositeFormat.Parse(AnswersData.PASSPORT_DATA_TEMPLATE_TEXT);

        public ProcessPassportDataCorrectionMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            DocumentsService documentsService,
            OpenAIService openAiService,
            IOptions<BotConfiguration> botConfig,
            SecretCache secretCache) : base(userService, botClient, documentsService, openAiService, botConfig, secretCache)
        {
        }

        protected override List<DocumentFieldModel<IdDocument>> GetInvalidations(IdDocument document)
        {
            return document.GetInvalidFieldsHandlers();
        }

        protected override async Task HandleInputStateChange(Message update, string cacheKey, CancellationToken cancellationToken)
        {
            if(update.From == null)
            {
                return;
            }

            await _userService.SetUserInputStateAsync(update.From.Id, uis =>
            {
                uis.CreateInsuranceFlow.IdCacheKey = cacheKey;
            }, cancellationToken);
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

        protected override async Task OnNoInvalidations(Message update, IdDocument document, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            await _botClient.SendMessage(
                update.Chat,
                string.Format(
                    CultureInfo.InvariantCulture,
                    _passportDataFormat,
                    document.DocumentNumber,
                    document.CountryCode,
                    document.Surname,
                    document.Name,
                    document.BirthDate.ToString("yyyy.MM.dd"),
                    document.ExpiryDate.ToString("yyyy.MM.dd")),
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PassportDataConfirmationAwait, update.From.Id, cancellationToken);
        }

        protected override async Task<IdDocument?> RetreiveDocument(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return null;
            }

            var data = await _documentsService.GetDataForUserAsync(update.From.Id, cancellationToken);

            return data?.idDocument ?? null;
        }
    }
}
