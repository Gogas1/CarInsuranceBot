using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Models.Documents;
using CarInsuranceBot.Core.Services;
using CarInsuranceBot.Core.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mindee;
using Mindee.Input;
using Mindee.Parsing;
using Mindee.Product.DriverLicense;
using Mindee.Product.InternationalId;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Passport;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.ReplyMarkups;
using CarInsuranceBot.Core.Constants;

namespace CarInsuranceBot.Core.Actions.MessageActions.DocumentsAwait
{
    internal class ProcessDocumentsDataAction : MessageActionBase
    {
        private readonly BotConfiguration _botConfig;
        private readonly MindeeClient _mindeeClient;
        private readonly SecretCache _secretCache;
        private readonly OpenAIService _openAiService;
        private readonly DocumentsService _documentsService;

        public ProcessDocumentsDataAction(
            UserService userService,
            ITelegramBotClient botClient,
            IOptions<BotConfiguration> botOptions,
            MindeeClient mindeeClient,
            SecretCache secretCache,
            OpenAIService openAiService,
            DocumentsService documentsService) : base(userService, botClient)
        {
            _botConfig = botOptions.Value;
            _mindeeClient = mindeeClient;
            _secretCache = secretCache;
            _openAiService = openAiService;
            _documentsService = documentsService;
        }

        protected override async Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            var passportData = update.PassportData;
            if (passportData == null)
            {
                await _botClient.SendMessage(
                    update.From.Id,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_DOCUMENTS_PROVIDED_SETTINGS, cancellationToken),
                    cancellationToken: cancellationToken);
                return;
            }

            var credentials = DecryptCredentials(passportData.Credentials, cancellationToken);
            if (credentials == null)
            {
                await _botClient.SendMessage(
                    update.From.Id,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_CREDENTIALS_SETTINGS, cancellationToken),
                    cancellationToken: cancellationToken);
                return;
            }

            if (!ValidateNonce(update.From.Id, credentials.Nonce))
            {
                await RequestPassportScopeAsync(
                    update.From.Id,
                    AnswersData.NO_NONCE_TEXT,
                    cancellationToken);
                return;
            }

            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.START_PROCESSING_SETTINGS, cancellationToken),
                cancellationToken: cancellationToken);
            await _botClient.SendChatAction(update.From.Id, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: cancellationToken);

            IdDocument? idDocumentData = await ProcessDocumentAsync<IdDocument, InternationalIdV2>(
                passportData.Data,
                EncryptedPassportElementType.Passport,
                credentials.SecureData.Passport?.FrontSide ?? null,
                document => new IdDocument
                {
                    DocumentNumber = document.Prediction.DocumentNumber.Value,
                    CountryCode = document.Prediction.CountryOfIssue.Value,
                    Surnames = document.Prediction.Surnames.Select(s => s.Value).ToList(),
                    Names = document.Prediction.GivenNames.Select(n => n.Value).ToList(),
                    BirthDate = document.Prediction.BirthDate.DateObject ?? DateTime.MinValue,
                    ExpiryDate = document.Prediction.ExpiryDate.DateObject ?? DateTime.MinValue,
                },
                d => d?.IsValid() ?? false,
                "passport.jpg",
                update.From.Id,
                cancellationToken);

            DriverLicenseDocument? licenseDocumentData = await ProcessDocumentAsync<DriverLicenseDocument, DriverLicenseV1>(
                passportData.Data,
                EncryptedPassportElementType.DriverLicense,
                credentials.SecureData.DriverLicense?.FrontSide ?? null,
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
                d => d?.IsValid() ?? false,
                "license.jpg",
                update.From.Id,
                cancellationToken);

            if (idDocumentData == null || licenseDocumentData == null)
            {
                await RequestPassportScopeAsync(
                    update.From.Id,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_DOCUMENT_DATA_SETTINGS, cancellationToken),
                    cancellationToken);
                return;
            }

            await SendDocumentsProvidedAsync(update.From.Id, idDocumentData, licenseDocumentData, cancellationToken);
        }

        private async Task SendDocumentsProvidedAsync(long userId, IdDocument idDoc, DriverLicenseDocument dlDoc, CancellationToken cancellationToken)
        {
            var idKey = await _secretCache.StoreAsync(idDoc, TimeSpan.FromMinutes(30));
            var dlKey = await _secretCache.StoreAsync(dlDoc, TimeSpan.FromMinutes(30));

            var message = string.Format(
                AnswersData.DOCUMENTS_PROVIDED_TEXT,
                idDoc.DocumentNumber,
                idDoc.CountryCode,
                idDoc.Surnames.First(),
                idDoc.Names.First(),
                idDoc.BirthDate.ToString("yyyy.MM.dd"),
                idDoc.ExpiryDate.ToString("yyyy.MM.dd"),

                dlDoc.Id,
                dlDoc.CountryCode,
                dlDoc.Category,
                dlDoc.LastName,
                dlDoc.FirstName,
                dlDoc.BirthDate.ToString("yyyy.MM.dd"),
                dlDoc.ExpiryDate.ToString("yyyy.MM.dd")
            );

            

            await _userService.SetUserInputStateAsync(userId, uis =>
            {
                uis.CreateInsuranceFlow.IdCacheKey = idKey;
                uis.CreateInsuranceFlow.DriverLicenseCacheKey = dlKey;
            }, cancellationToken);

            await _botClient.SendMessage(
                userId,
                message,
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsDataConfirmationAwait, userId, cancellationToken);
        }

        private async Task RequestPassportScopeAsync(long userId, string text, CancellationToken cancellationToken)
        {
            var newNonce = _documentsService.SetNonceForUser(userId);

            await _botClient.SendMessage(
                userId,
                text,
                replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfig, newNonce),
                cancellationToken: cancellationToken);
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, userId, cancellationToken);
        }

        private bool ValidateNonce(long userId, string nonce)
        {
            var cached = _documentsService.GetCachedNonce(userId);
            return cached == nonce;
        }

        private Credentials? DecryptCredentials(EncryptedCredentials credentials, CancellationToken cancellationToken)
        {
            var decrypter = new Decrypter();
            return decrypter.DecryptCredentials(credentials, RSAUtils.GetRSAFromString(_botConfig.Private256Key));
        }

        private async Task<TDocument?> ProcessDocumentAsync<TDocument, TMindeeDocument>(
            IEnumerable<EncryptedPassportElement> elements, 
            EncryptedPassportElementType type, 
            FileCredentials? fileCredentials,
            Func<TMindeeDocument, TDocument?> mappingFunction,
            Func<TDocument?, bool> validationFunction,
            string fileName,
            long userId, 
            CancellationToken cancellationToken) where TMindeeDocument : class, new() where TDocument : class, new()
        {
            if(fileCredentials == null)
            {
                return null;
            }

            var element = elements.SingleOrDefault(e => e.Type == type);
            if (element?.FrontSide == null)
                return null;

            using var stream = new MemoryStream();
            await _botClient.DownloadAndDecryptPassportFileAsync(
                element.FrontSide,
                fileCredentials,
                stream,
                cancellationToken);
            stream.Position = 0;

            var response = await _mindeeClient.EnqueueAndParseAsync<TMindeeDocument>(
                new LocalInputSource(stream, fileName));
            cancellationToken.ThrowIfCancellationRequested();

            if (response.ApiRequest.StatusCode != 200)
            {
                await SendExtractionErrorAsync(type, fileCredentials.FileHash, userId, cancellationToken: cancellationToken);
                return null;
            }

            var doc = mappingFunction(response.Document.Inference);

            if(!validationFunction(doc))
            {
                await SendExtractionErrorAsync(type, fileCredentials.FileHash, userId, cancellationToken: cancellationToken);
                return null;
            }

            return doc;
        }

        private async Task SendExtractionErrorAsync(EncryptedPassportElementType type, string fileHash, long userId, CancellationToken cancellationToken)
        {
            var error = new PassportElementErrorFrontSide
            {
                Type = type,
                FileHash = fileHash,
                Message = AnswersData.NO_DOCUMENTS_DATA_TEXT,
            };
            await _botClient.SetPassportDataErrors(userId, [error], cancellationToken);
        }

        
    }
}
