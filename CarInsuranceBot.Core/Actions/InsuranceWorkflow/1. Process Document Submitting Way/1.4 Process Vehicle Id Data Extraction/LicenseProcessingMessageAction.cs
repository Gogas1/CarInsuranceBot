using CarInsuranceBot.Core.Actions.Abstractions;
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
using Telegram.Bot.Types.Passport;

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    internal class LicenseProcessingMessageAction : ChatDocumentProcessingAction
    {
        private SecretCache _secretCache;
        private DocumentsService _documentsService;
        private BotConfiguration _botConfig;

        private static readonly CompositeFormat _vehicleIdDataFormat =
       CompositeFormat.Parse(AnswersData.VEHICLE_ID_DATA_TEMPLATE_TEXT);

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

            if (!driverLicenseData.IsValid())
            {
                await OnExtractionError(update, driverLicenseData, cancellationToken);
                return;
            }

            var message = string.Format(
                CultureInfo.InvariantCulture,
                _vehicleIdDataFormat,
                driverLicenseData.RegistrationNumber,
                driverLicenseData.RegistrationDate.ToString("yyyy.MM.dd")
            );

            await _botClient.SendMessage(
                update.From.Id,
                message,
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.LicenseDataConfirmationAwait, update.From.Id, cancellationToken);
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
                    RegistrationNumber = document.Prediction.Id.Value,
                    RegistrationDate = document.Prediction.IssuedDate.DateObject ?? DateTime.MinValue,
                },
                async _ => await OnExtractionError(
                    update,
                    null,
                    cancellationToken),
                async doc => await OnExtractionError(
                    update,
                    doc,
                    cancellationToken),
                cancellationToken
                );
        }

        protected async Task OnExtractionError(Message update, DriverLicenseDocument? document, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            document ??= new DriverLicenseDocument();
            var idKey = await _secretCache.StoreAsync(document, TimeSpan.FromMinutes(30));

            await _userService.SetUserInputStateAsync(update.From.Id, uis =>
            {
                uis.CreateInsuranceFlow.DriverLicenseCacheKey = idKey;
            }, cancellationToken);

            var invalidations = document.GetInvalidFieldsHandlers();
            var firstInvalidation = invalidations.First();

            await _botClient.SendMessage(
                update.Chat,
                await _openAiService.GetDiversifiedAnswer(AnswersData.NO_DOCUMENT_DATA_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.CORRECTNESS_PROCESSING_KEYBOARD,
                cancellationToken: cancellationToken);
            await _botClient.SendMessage(update.Chat, $"You need to fill up the \"{firstInvalidation.Name}\" field first");
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.LicenseDataCorrectionAwait, update.From.Id, cancellationToken);
        }
    }
}
