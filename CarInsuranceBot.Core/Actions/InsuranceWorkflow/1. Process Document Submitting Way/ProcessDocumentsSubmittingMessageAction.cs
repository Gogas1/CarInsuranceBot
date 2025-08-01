﻿using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.Core.Models.Documents;
using CarInsuranceBot.Core.Services;
using CarInsuranceBot.Core.Utils;
using Microsoft.Extensions.Options;
using Mindee;
using Mindee.Input;
using Mindee.Product.DriverLicense;
using Mindee.Product.InternationalId;
using System.Globalization;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Passport;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Passport;

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    /// <summary>
    /// <see cref="MessageActionBase"/> implementation action to process user documents submitting
    /// <para>Exit states: 
    /// <see cref="UserState.DocumentsDataConfirmationAwait"/> if documents data extracted, 
    /// <see cref="UserState.DocumentsAwait"/> if documents processing error,
    /// </summary>
    internal class ProcessDocumentsSubmittingMessageAction : GeneralInformationalMessageAction
    {
        private readonly BotConfiguration _botConfig;
        private readonly MindeeClient _mindeeClient;
        private readonly SecretCache _secretCache;
        private readonly DocumentsService _documentsService;

        protected override TimeSpan Timeout => TimeSpan.FromSeconds(20);
        private static readonly CompositeFormat _documentsProvidedFormat =
        CompositeFormat.Parse(AnswersData.DOCUMENTS_PROVIDED_TEXT);
        public ProcessDocumentsSubmittingMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            IOptions<BotConfiguration> botOptions,
            MindeeClient mindeeClient,
            SecretCache secretCache,
            OpenAIService openAiService,
            DocumentsService documentsService) : base(userService, botClient, openAiService)
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

            OpenAIService.SelectItem? selectedOption = null;
            // Init default options
            OpenAIService.SelectItem defautOption = new OpenAIService.SelectItem(
                -1,
                "Process documents",
                async _ => await OnProcessDocumentsWay(update, cancellationToken));

            //Init options list
            List<OpenAIService.SelectItem> options = [
                new OpenAIService.SelectItem(
                    0,
                    AnswersData.INSURANCE_ORDERING_DOCUMENTS_IN_CHAT_OPTION,
                    async _ => await OnShareInChat(update, cancellationToken)),
                new OpenAIService.SelectItem(
                    1,
                    AnswersData.INSURANCE_ORDERING_RECONSIDERATION_OPTION,
                    async _ => await OnReconsideration(update, cancellationToken)),
                ];

            // If user wrote something
            if (update.Text != null)
            {

                options.AddRange(CreateBaseQuestionsOptionsList(update, AnswersData.DOCUMENTS_AWAIT_STATE_GUIDANCE, cancellationToken: cancellationToken));
                // Get selected option by GPT and execute
                selectedOption = await _openAiService.GetSelectionByTextAsync(options, defautOption, update.Text.Truncate(100), cancellationToken);
                selectedOption.OnSelection();

                return;
            }

            // Otherwise default option
            defautOption.OnSelection();
        }

        private async Task OnShareInChat(Message update, CancellationToken cancellationToken)
        {
            if(update.From == null)
            {
                return;
            }

            await _botClient.SendMessage(update.Chat, await _openAiService.GetDiversifiedAnswer(AnswersData.SHARE_PASSPORT_IN_CHAT_GPT_SETTINGS, cancellationToken));
            await _userService.SetUserStateByTelegramIdAsync(UserState.PassportAwait, update.From.Id, cancellationToken);
        }

        private async Task OnReconsideration(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            //Send message about reconsideration with home keyboard markup
            await _botClient.SendMessage(
                update.Chat,
                AnswersData.USER_RECONSIDERED_ANSWER_TEXT,
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);

            //Change user state to UserState.Home
            await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id, cancellationToken: cancellationToken);
        }

        private async Task OnProcessDocumentsWay(Message update, CancellationToken cancellationToken)
        {
            if(update.From == null)
            {
                return;
            }

            // Is message contains passport data check
            var passportData = update.PassportData;
            if (passportData == null)
            {
                // If not - send message about error and repeat passport request
                await RequestPassportScopeAsync(
                    update.From.Id,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_DOCUMENTS_PROVIDED_SETTINGS, cancellationToken),
                    cancellationToken);
                return;
            }

            // Is appication can retreive credentials data from the passport data check
            var credentials = DecryptCredentials(passportData.Credentials, cancellationToken);
            if (credentials == null)
            {
                // If not - send message about error and repeat passport request
                await RequestPassportScopeAsync(
                    update.From.Id,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_CREDENTIALS_SETTINGS, cancellationToken),
                    cancellationToken);
                return;
            }

            // Is nonce valid
            if (!ValidateNonce(update.From.Id, credentials.Nonce))
            {
                // If not - send message about error and repeat passport request
                await RequestPassportScopeAsync(
                    update.From.Id,
                    AnswersData.NO_NONCE_TEXT,
                    cancellationToken);
                return;
            }

            // Send message about starting of processing
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.START_PROCESSING_SETTINGS, cancellationToken),
                cancellationToken: cancellationToken);

            // Set chat action
            await _botClient.SendChatAction(update.From.Id, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: cancellationToken);

            // Process id documents data
            IdDocument? idDocumentData = await ProcessDocumentAsync<IdDocument, InternationalIdV2>(
                //Pass passport data
                passportData.Data,
                // Pass type selector
                EncryptedPassportElementType.Passport,
                // Pass credentials of the passport front side
                credentials.SecureData.Passport?.FrontSide ?? null,
                // Pass IdDocument factory based on Mindee document
                document => new IdDocument
                {
                    DocumentNumber = document.Prediction.DocumentNumber.Value,
                    CountryCode = document.Prediction.CountryOfIssue.Value,
                    Surname = document.Prediction.Surnames.Select(s => s.Value).FirstOrDefault() ?? string.Empty,
                    Name = document.Prediction.GivenNames.Select(n => n.Value).FirstOrDefault() ?? string.Empty,
                    BirthDate = document.Prediction.BirthDate.DateObject ?? DateTime.MinValue,
                    ExpiryDate = document.Prediction.ExpiryDate.DateObject ?? DateTime.MinValue,
                },
                // Pass validation function
                d => d?.IsValid() ?? false,
                // File name for the Mindee api
                "passport.jpg",
                // User id for the feedback
                update.From.Id,
                cancellationToken);

            // Process driver license data
            DriverLicenseDocument? licenseDocumentData = await ProcessDocumentAsync<DriverLicenseDocument, DriverLicenseV1>(
                //Pass passport data
                passportData.Data,
                // Pass type selector
                EncryptedPassportElementType.DriverLicense,
                // Pass credentials of the driver license front side
                credentials.SecureData.DriverLicense?.FrontSide ?? null,
                // Pass DriverLicenseDocument factory based on Mindee document
                document => new DriverLicenseDocument
                {
                    
                },
                // Pass validation function
                d => true,
                // File name for the Mindee api
                "license.jpg",
                // User id for the feedback
                update.From.Id,
                cancellationToken);

            //If data not present
            if (idDocumentData == null || licenseDocumentData == null)
            {
                // Send error message and preat passport request
                await RequestPassportScopeAsync(
                    update.From.Id,
                    await _openAiService.GetDiversifiedAnswer(AnswersData.NO_DOCUMENT_DATA_SETTINGS, cancellationToken),
                    cancellationToken);
                return;
            }

            // Cache documents data and proceed
            await SendDocumentsProvidedAsync(update.From.Id, idDocumentData, licenseDocumentData, cancellationToken);
        }

        /// <summary>
        /// Process documents data successful processing
        /// </summary>
        /// <param name="userId">User telegram id</param>
        /// <param name="idDoc">ID/Passport document data</param>
        /// <param name="dlDoc">Driver license document data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SendDocumentsProvidedAsync(long userId, IdDocument idDoc, DriverLicenseDocument dlDoc, CancellationToken cancellationToken)
        {
            // Cache document data
            var idKey = await _secretCache.StoreAsync(idDoc, TimeSpan.FromMinutes(30));
            var dlKey = await _secretCache.StoreAsync(dlDoc, TimeSpan.FromMinutes(30));

            // Create documents data confirmation message text
            var message = string.Format(
                CultureInfo.InvariantCulture,
                _documentsProvidedFormat,
                idDoc.DocumentNumber,
                idDoc.CountryCode,
                idDoc.Surname,
                idDoc.Name,
                idDoc.BirthDate.ToString("yyyy.MM.dd"),
                idDoc.ExpiryDate.ToString("yyyy.MM.dd"),

                dlDoc.RegistrationNumber,
                dlDoc.RegistrationDate.ToString("yyyy.MM.dd")
            );

            // Store user input data - cache keys
            await _userService.SetUserInputStateAsync(userId, uis =>
            {
                uis.CreateInsuranceFlow.IdCacheKey = idKey;
                uis.CreateInsuranceFlow.DriverLicenseCacheKey = dlKey;
            }, cancellationToken);

            //Send documents data confirmation message and keyboard
            await _botClient.SendMessage(
                userId,
                message,
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(UserState.DocumentsDataConfirmationAwait, userId, cancellationToken);
        }

        /// <summary>
        /// Sends message with authorization keyboard to request documents data
        /// </summary>
        /// <param name="userId">Telegram user id</param>
        /// <param name="text">Message text</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        private async Task RequestPassportScopeAsync(long userId, string text, CancellationToken cancellationToken)
        {
            // Create and set new nonce
            var newNonce = _documentsService.SetNonceForUser(userId);

            // Send message with authorization keyboard
            await _botClient.SendMessage(
                userId,
                text,
                replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfig, newNonce),
                cancellationToken: cancellationToken);

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(UserState.DocumentsAwait, userId, cancellationToken);
        }

        /// <summary>
        /// Validates nonce against cached nonce for user
        /// </summary>
        /// <param name="userId">Telegram user id</param>
        /// <param name="nonce">Nonce to validate</param>
        /// <returns></returns>
        private bool ValidateNonce(long userId, string nonce)
        {
            // Get cached nonce for user
            var cached = _documentsService.GetCachedNonce(userId);
            return cached == nonce;
        }

        /// <summary>
        /// Decrypts telegram passport credentials
        /// </summary>
        /// <param name="credentials">Encrypted credentialsl</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Decrypted credentials</returns>
        private Credentials? DecryptCredentials(EncryptedCredentials credentials, CancellationToken cancellationToken)
        {
            // Instantiate decrypter
            var decrypter = new Decrypter();
            // Decrypt using private key and return credentials
            return decrypter.DecryptCredentials(credentials, RSAUtils.GetRSAFromString(_botConfig.Private256Key));
        }

        /// <summary>
        /// Process document photo using Mindee and return mapped document data
        /// </summary>
        /// <typeparam name="TDocument">Document data map type</typeparam>
        /// <typeparam name="TMindeeDocument">Target Mindee document</typeparam>
        /// <param name="elements">Telegram documents data collection</param>
        /// <param name="type">Telegram document type</param>
        /// <param name="fileCredentials">Telegram file credentials of the target document</param>
        /// <param name="mappingFunction">Mapping function from the Mindee document to the application model</param>
        /// <param name="validationFunction">Application document model validation function</param>
        /// <param name="fileName">Filename for the Mindee API</param>
        /// <param name="userId">Telegram user id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Target document application model</returns>
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
            if (fileCredentials == null)
            {
                return null;
            }

            // Get target encrypted passport element
            var element = elements.SingleOrDefault(e => e.Type == type);
            if (element?.FrontSide == null)
                return null;

            // Create stream to handle downloading of the document photo and uploading to the Mindee API
            using var stream = new MemoryStream();

            //Download and decrypt file
            await _botClient.DownloadAndDecryptPassportFileAsync(
                element.FrontSide,
                fileCredentials,
                stream,
                cancellationToken);
            stream.Position = 0;

            // Extract data using Mindee
            var response = await _mindeeClient.EnqueueAndParseAsync<TMindeeDocument>(
                new LocalInputSource(stream, fileName));
            cancellationToken.ThrowIfCancellationRequested();

            // Handle error
            if (response.ApiRequest.StatusCode != 200)
            {
                await SendExtractionErrorAsync(type, fileCredentials.FileHash, userId, cancellationToken: cancellationToken);
                return null;
            }

            var doc = mappingFunction(response.Document.Inference);

            // Handle invalidation
            if (!validationFunction(doc))
            {
                await SendExtractionErrorAsync(type, fileCredentials.FileHash, userId, cancellationToken: cancellationToken);
                return null;
            }

            // Return mapped document
            return doc;
        }

        /// <summary>
        /// Sends telegram passport file error against target element
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileHash"></param>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        // Handle timeout
        protected override async Task OnTimeoutAsync(Message update)
        {
            await base.OnTimeoutAsync(update);

            if(update.From == null)
            {
                return;
            }

            // Create and set new nonce for user
            var nonce = _documentsService.SetNonceForUser(update.From.Id);

            // Send message about submitting the documents and authorization keyboard
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.START_INSURANCE_WORKFLOW_SETTINGS, default),
                replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfig, nonce));

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(UserState.DocumentsAwait, update.From.Id, default);
        }
    }
}
