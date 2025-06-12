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
        private readonly MemoryCache _cache;
        private readonly MindeeClient _mindeeClient;
        private readonly SecretCache _secretCache;

        public ProcessDocumentsDataAction(
            UserService userService,
            ITelegramBotClient botClient,
            IOptions<BotConfiguration> botOptions,
            MemoryCache cache,
            MindeeClient mindeeClient,
            SecretCache secretCache) : base(userService, botClient)
        {
            _botConfig = botOptions.Value;
            _cache = cache;
            _mindeeClient = mindeeClient;
            _secretCache = secretCache;
        }

        public override async Task Execute(Message update)
        {
            if (update.From == null)
            {
                return;
            }

            await base.Execute(update);

            if(IsCancellationRequested)
            {
                return;
            }

            var passportData = update.PassportData;
            if (passportData == null)
            {
                await _botClient.SendMessage(update.From.Id, AnswersData.NoFileMessageFallbackText);
                return;
            }

            var credentials = DecryptCredentials(passportData.Credentials);
            if (credentials == null)
            {
                await _botClient.SendMessage(update.From.Id, AnswersData.NoCredentialsMessageFallbackText);
                return;
            }

            if (!ValidateNonce(update.From.Id, credentials.Nonce))
            {
                await RequestPassportScopeAsync(update.From.Id, AnswersData.NoNonceMessageFallbackText);
                return;
            }

            await _botClient.SendMessage(update.From.Id, AnswersData.StartProcessingFallbackText);
            await _botClient.SendChatAction(update.From.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.Processing, update.From.Id);

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
                update.From.Id);

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
                update.From.Id);

            if(idDocumentData == null || licenseDocumentData == null)
            {
                await RequestPassportScopeAsync(update.From.Id, AnswersData.NoDocumentsFallbackText);
                return;
            }

            await SendDocumentsProvidedAsync(update.From.Id, idDocumentData, licenseDocumentData);
        }

        private async Task SendDocumentsProvidedAsync(long userId, IdDocument idDoc, DriverLicenseDocument dlDoc)
        {
            var idKey = await _secretCache.StoreAsync(idDoc, TimeSpan.FromMinutes(30));
            var dlKey = await _secretCache.StoreAsync(dlDoc, TimeSpan.FromMinutes(30));

            var message = string.Format(
                AnswersData.DocumentsProvidedFallbackText,
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

            

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsDataConfirmationAwait, userId);
            await _userService.SetUserInputStateAsync(userId, uis =>
            {
                uis.CreateInsuranceFlow.IdCacheKey = idKey;
                uis.CreateInsuranceFlow.DriverLicenseCacheKey = dlKey;
            });

            await _botClient.SendMessage(userId, message, replyMarkup: AnswersData.DataConfirmationKeyboard);
        }

        private async Task RequestPassportScopeAsync(long userId, string text)
        {
            var newNonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));
            var authReq = AnswersData.GetAuthorizationRequestParameters(_botClient, _botConfig, newNonce);

            _cache.Set($"nonce_{userId}", newNonce, TimeSpan.FromMinutes(10));
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, userId);
            await _botClient.SendMessage(
                userId,
                text,
                replyMarkup: InlineKeyboardButton.WithUrl(
                    AnswersData.ShareDocumentButtonText,
                    string.Format(AnswersData.RedirectUrl, authReq.Query)));
        }

        private bool ValidateNonce(long userId, string nonce)
        {
            var cached = _cache.Get($"nonce_{userId}") as string;
            return cached == nonce;
        }

        private Credentials? DecryptCredentials(EncryptedCredentials credentials)
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
            long userId) where TMindeeDocument : class, new() where TDocument : class, new()
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
                stream);
            stream.Position = 0;

            var response = await _mindeeClient.EnqueueAndParseAsync<TMindeeDocument>(
                new LocalInputSource(stream, fileName));

            if (response.ApiRequest.StatusCode != 200)
            {
                await SendExtractionErrorAsync(type, fileCredentials.FileHash, userId);
                return null;
            }

            var doc = mappingFunction(response.Document.Inference);

            if(!validationFunction(doc))
            {
                await SendExtractionErrorAsync(type, fileCredentials.FileHash, userId);
                return null;
            }

            return doc;
        }

        private Task SendExtractionErrorAsync(EncryptedPassportElementType type, string fileHash, long userId)
        {
            var error = new PassportElementErrorFrontSide
            {
                Type = type,
                FileHash = fileHash,
                Message = AnswersData.NoIdDataFallbackText,
            };
            return _botClient.SetPassportDataErrors(userId, [error]);
        }
    }
}
