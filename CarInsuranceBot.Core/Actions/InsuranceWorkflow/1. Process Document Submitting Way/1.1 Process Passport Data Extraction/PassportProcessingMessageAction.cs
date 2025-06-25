using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.Core.Models.Documents;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Mindee;
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
    internal class PassportProcessingMessageAction : ChatDocumentProcessingAction
    {
        private SecretCache _secretCache;


        private static readonly CompositeFormat _passportDataFormat =
       CompositeFormat.Parse(AnswersData.PASSPORT_DATA_TEMPLATE_TEXT);

        public PassportProcessingMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAiService,
            MindeeClient mindeeClient,
            SecretCache secretCache) : base(userService, botClient, openAiService, mindeeClient)
        {
            _secretCache = secretCache;
        }

        protected override async Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            if (update.Photo == null || update.Photo.Length == 0)
            {
                await ProcessNoPhoto(update, AnswersData.PASSPORT_AWAIT_STATE_GUIDANCE, await _openAiService.GetDiversifiedAnswer(AnswersData.SHARE_PASSPORT_IN_CHAT_GPT_SETTINGS, cancellationToken), cancellationToken);
                return;
            }

            IdDocument? idDocumentData = await ProcessDocument(update, cancellationToken);

            if (idDocumentData == null)
            {
                return;
            }

            await ProcessExtractionSuccess(update, idDocumentData, cancellationToken);
        }

        private async Task ProcessExtractionSuccess(Message update, IdDocument idDocumentData, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            var documentCacheKey = await _secretCache.StoreAsync(idDocumentData, TimeSpan.FromMinutes(30));

            await _userService.SetUserInputStateAsync(update.From.Id, uis =>
            {
                uis.CreateInsuranceFlow.IdCacheKey = documentCacheKey;
            }, cancellationToken);

            if(!idDocumentData.IsValid())
            {
                await OnExtractionError(update, idDocumentData, cancellationToken);
                return;
            }

            await _botClient.SendMessage(
                update.Chat,
                string.Format(
                    CultureInfo.InvariantCulture,
                    _passportDataFormat,
                    idDocumentData.DocumentNumber,
                    idDocumentData.CountryCode,
                    idDocumentData.Surname,
                    idDocumentData.Name,
                    idDocumentData.BirthDate.ToString("yyyy.MM.dd"),
                    idDocumentData.ExpiryDate.ToString("yyyy.MM.dd")),
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PassportDataConfirmationAwait, update.From.Id, cancellationToken);
        }

        private async Task<IdDocument?> ProcessDocument(Message update, CancellationToken cancellationToken)
        {
            if(update.From == null || update.Photo == null)
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
            return await ProcessDocumentAsync<IdDocument, InternationalIdV2>(
                photoStream,
                update.From.Id,
                "passport.jpg",
                document => new IdDocument
                {
                    DocumentNumber = document.Prediction.DocumentNumber.Value,
                    CountryCode = document.Prediction.CountryOfIssue.Value,
                    Surname = document.Prediction.Surnames.Select(s => s.Value).FirstOrDefault() ?? string.Empty,
                    Name = document.Prediction.GivenNames.Select(n => n.Value).FirstOrDefault() ?? string.Empty,
                    BirthDate = document.Prediction.BirthDate.DateObject ?? DateTime.MinValue,
                    ExpiryDate = document.Prediction.ExpiryDate.DateObject ?? DateTime.MinValue,
                },
                async _ => await OnExtractionError(update, null, cancellationToken),
                async doc => await OnExtractionError(update, doc, cancellationToken),
                cancellationToken
                );
        }

        protected async Task OnExtractionError(Message update, IdDocument? document, CancellationToken cancellationToken) {
            if(update.From == null)
            {
                return;
            }

            document ??= new IdDocument();
            var idKey = await _secretCache.StoreAsync(document, TimeSpan.FromMinutes(30));

            await _userService.SetUserInputStateAsync(update.From.Id, uis =>
            {
                uis.CreateInsuranceFlow.IdCacheKey = idKey;
            }, cancellationToken);

            var invalidations = document.GetInvalidFieldsHandlers();
            var firstInvalidation = invalidations.First();

            await _botClient.SendMessage(
                update.Chat,
                await _openAiService.GetDiversifiedAnswer(AnswersData.NO_DOCUMENT_DATA_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.CORRECTNESS_PROCESSING_KEYBOARD,
                cancellationToken: cancellationToken);
            await _botClient.SendMessage(update.Chat, $"For the manual information entering you need to fill up the \"{firstInvalidation.Name}\" field first");
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PassportDataCorrectionAwait, update.From.Id, cancellationToken);
        }
    }
}
