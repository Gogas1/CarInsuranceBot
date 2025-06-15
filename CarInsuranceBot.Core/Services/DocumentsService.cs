using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Models;
using CarInsuranceBot.Core.Models.Documents;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Services
{
    public class DocumentsService
    {
        private readonly SecretCache _secretCache;
        private readonly UserService _userService;
        private readonly MemoryCache _cache;
        private readonly ITelegramBotClient _botClient;

        public DocumentsService(IServiceProvider serviceProvider)
        {
            _secretCache = serviceProvider.GetRequiredService<SecretCache>();
            _userService = serviceProvider.GetRequiredService<UserService>();
            _botClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
            _cache = serviceProvider.GetRequiredService<MemoryCache>();
        }

        internal DocumentsService(SecretCache secretCache, UserService userService, ITelegramBotClient botClient, MemoryCache cache)
        {
            _secretCache = secretCache;
            _userService = userService;
            _botClient = botClient;
            _cache = cache;
        }

        internal string SetNonceForUser(long userId)
        {
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));
            _cache.Set($"nonce_{userId}", nonce, TimeSpan.FromMinutes(10));

            return nonce;
        }

        internal string GetCachedNonce(long userId)
        {
            var nonceObject = _cache.Get($"nonce_{userId}");
            if(nonceObject is string nonce)
            {
                return nonce;
            }

            return string.Empty;
        }

        internal async Task<UserDocuments?> GetDataForUserAsync(long userId, CancellationToken cancellationToken)
        {
            var userInputState = await _userService.GetUserInputStateAsync(userId, cancellationToken);
            if(string.IsNullOrEmpty(userInputState.CreateInsuranceFlow.IdCacheKey) || string.IsNullOrEmpty(userInputState.CreateInsuranceFlow.DriverLicenseCacheKey))
            {
                return null;
            }

            var idData = await _secretCache.RetrieveAsync<IdDocument>(userInputState.CreateInsuranceFlow.IdCacheKey);
            var licenseData = await _secretCache.RetrieveAsync<DriverLicenseDocument>(userInputState.CreateInsuranceFlow.DriverLicenseCacheKey);

            if (idData == null || licenseData == null)
            {
                return null;
            }

            return new UserDocuments(idData, licenseData);
        }

        internal async Task DeleteUserDataAsync(long userId, CancellationToken cancellationToken)
        {
            var userInputState = await _userService.GetUserInputStateAsync(userId, cancellationToken);
            if (string.IsNullOrEmpty(userInputState.CreateInsuranceFlow.IdCacheKey) || string.IsNullOrEmpty(userInputState.CreateInsuranceFlow.DriverLicenseCacheKey))
            {
                return;
            }

            await _secretCache.DeleteAsync(userInputState.CreateInsuranceFlow.IdCacheKey);
            await _secretCache.DeleteAsync(userInputState.CreateInsuranceFlow.DriverLicenseCacheKey);
        }

        public async Task SetDocumentsForUser(long userId, IdDocument idDocument, DriverLicenseDocument licenseDocument, CancellationToken cancellationToken)
        {
            var idKey = await _secretCache.StoreAsync(idDocument, TimeSpan.FromMinutes(30));
            var dlKey = await _secretCache.StoreAsync(licenseDocument, TimeSpan.FromMinutes(30));

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsDataConfirmationAwait, userId, cancellationToken);
            await _userService.SetUserInputStateAsync(userId, uis =>
            {
                uis.CreateInsuranceFlow.IdCacheKey = idKey;
                uis.CreateInsuranceFlow.DriverLicenseCacheKey = dlKey;
            }, cancellationToken);

            await _botClient.SendMessage(userId, "Do you confirm data?", replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD);
        }

        internal class UserDocuments
        {
            public UserDocuments(IdDocument idDocument, DriverLicenseDocument driverLicenseDocument)
            {
                this.idDocument = idDocument;
                DriverLicenseDocument = driverLicenseDocument;
            }

            public IdDocument idDocument { get; }
            public DriverLicenseDocument DriverLicenseDocument { get; }
        }
    }
}
