using CarInsuranceBot.Core.Actions.MessageActions.Abstractions;
using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.Core.Models.Documents;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Mindee;
using Mindee.Product.DriverLicense;
using Mindee.Product.InternationalId;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions.LicenseAwait
{
    internal class LicenseProcessingMessageAction : ChatDocumentProcessingAction
    {
        private SecretCache _secretCache;
        private DocumentsService _documentsService;
        private BotConfiguration _botConfig;

        public LicenseProcessingMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAiService,
            MindeeClient mindeeClient,
            SecretCache secretCache,
            DocumentsService documentsService,
            IOptions<BotConfiguration> botConfig) : base(userService, botClient, openAiService, mindeeClient)
        {
            _secretCache = secretCache;
            _documentsService = documentsService;
            _botConfig = botConfig.Value;
        }
        private static readonly CompositeFormat _documentsProvidedFormat =
        CompositeFormat.Parse(AnswersData.DOCUMENTS_PROVIDED_TEXT);
        protected override async Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            if (update.Photo == null || update.Photo.Length == 0)
            {
                await ProcessNoPhoto(update, AnswersData.LICENSE_AWAIT_STATE_GUIDANCE, await _openAiService.GetDiversifiedAnswer(AnswersData.SHARE_LICENSE_IN_CHAT_GPT_SETTINGS, cancellationToken), cancellationToken);
                return;
            }

            DriverLicenseDocument? driverLicenseData = await ProcessDocument(update, cancellationToken);

            if (driverLicenseData == null)
            {
                return;
            }

            await ProcessExtractionSuccess(update, driverLicenseData, cancellationToken);
        }

        private async Task ProcessExtractionSuccess(Message update, DriverLicenseDocument driverLicenseData, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            var driverLicenseKey = await _secretCache.StoreAsync(driverLicenseData, TimeSpan.FromMinutes(30));

            await _userService.SetUserInputStateAsync(update.From.Id, uis =>
            {
                uis.CreateInsuranceFlow.DriverLicenseCacheKey = driverLicenseKey;
            }, cancellationToken);

            var data = await _documentsService.GetDataForUserAsync(update.From.Id, cancellationToken);

            if(data == null)
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
                data.idDocument.Surnames.First(),
                data.idDocument.Names.First(),
                data.idDocument.BirthDate.ToString("yyyy.MM.dd"),
                data.idDocument.ExpiryDate.ToString("yyyy.MM.dd"),

                data.DriverLicenseDocument.Id,
                data.DriverLicenseDocument.CountryCode,
                data.DriverLicenseDocument.Category,
                data.DriverLicenseDocument.LastName,
                data.DriverLicenseDocument.FirstName,
                data.DriverLicenseDocument.BirthDate.ToString("yyyy.MM.dd"),
                data.DriverLicenseDocument.ExpiryDate.ToString("yyyy.MM.dd")
            );

            await _botClient.SendMessage(
                update.From.Id,
                message,
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsDataConfirmationAwait, update.From.Id, cancellationToken);
        }

        private async Task<DriverLicenseDocument?> ProcessDocument(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null || update.Photo == null)
            {
                return null;
            }

            var tgPhoto = update.Photo.Last();
            var tgFile = await _botClient.GetFile(tgPhoto.FileId);

            await using var photoStream = new MemoryStream();
            await _botClient.DownloadFile(tgFile, photoStream);

            await _botClient.SendMessage(
                update.Chat,
                await _openAiService.GetDiversifiedAnswer(AnswersData.START_PROCESSING_SETTINGS, cancellationToken),
                cancellationToken: cancellationToken);

            photoStream.Position = 0;
            return await ProcessDocumentAsync<DriverLicenseDocument, DriverLicenseV1>(
                photoStream,
                update.From.Id,
                "passport.jpg",
                document => new DriverLicenseDocument
                {
                    CountryCode = document.Prediction.CountryCode.Value,
                    Id = document.Prediction.Id.Value,
                    Category = document.Prediction.Category.Value,
                    LastName = document.Prediction.LastName.Value,
                    FirstName = document.Prediction.FirstName.Value,
                    ExpiryDate = document.Prediction.ExpiryDate.DateObject ?? DateTime.MinValue,
                    BirthDate = document.Prediction.DateOfBirth.DateObject ?? DateTime.MinValue,
                },
                // Pass validation function
                d => d?.IsValid() ?? false,
                async _ => await OnExtractionError(
                    update,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_DOCUMENT_DATA_SETTINGS, cancellationToken),
                    cancellationToken),
                async _ => await OnExtractionError(
                    update,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_DOCUMENT_DATA_SETTINGS, cancellationToken),
                    cancellationToken),
                cancellationToken
                );
        }

    }
}
