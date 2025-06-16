using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Models.Documents;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using Telegram.Bot;

namespace CarInsuranceBot.Core.Services
{
    /// <summary>
    /// Service to handle user documents workflow
    /// </summary>
    internal class DocumentsService
    {
        private readonly SecretCache _secretCache;
        private readonly UserService _userService;
        private readonly MemoryCache _cache;
        private readonly ITelegramBotClient _botClient;

        public DocumentsService(SecretCache secretCache, UserService userService, ITelegramBotClient botClient, MemoryCache cache)
        {
            _secretCache = secretCache;
            _userService = userService;
            _botClient = botClient;
            _cache = cache;
        }

        /// <summary>
        ///Creates and saves nonce for user         
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal string SetNonceForUser(long userId)
        {
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));
            _cache.Set($"nonce_{userId}", nonce, TimeSpan.FromMinutes(10));

            return nonce;
        }

        /// <summary>
        /// Retreives nonce for user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal string GetCachedNonce(long userId)
        {
            var nonceObject = _cache.Get($"nonce_{userId}");
            if (nonceObject is string nonce)
            {
                return nonce;
            }

            return string.Empty;
        }

        /// <summary>
        /// Retreives cached user documents data
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task<UserDocuments?> GetDataForUserAsync(long userId, CancellationToken cancellationToken)
        {
            var userInputState = await _userService.GetUserInputStateAsync(userId, cancellationToken);
            if (string.IsNullOrEmpty(userInputState.CreateInsuranceFlow.IdCacheKey) || string.IsNullOrEmpty(userInputState.CreateInsuranceFlow.DriverLicenseCacheKey))
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

        /// <summary>
        /// Deletes cached data of the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
