using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Models;
using CarInsuranceBot.Core.Models.Documents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ITelegramBotClient _botClient;

        public DocumentsService(IServiceProvider serviceProvider)
        {
            _secretCache = serviceProvider.GetRequiredService<SecretCache>();
            _userService = serviceProvider.GetRequiredService<UserService>();
            _botClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
        }

        internal DocumentsService(SecretCache secretCache, UserService userService, ITelegramBotClient botClient)
        {
            _secretCache = secretCache;
            _userService = userService;
            _botClient = botClient;
        }

        internal async Task<UserDocuments?> GetDataForUserAsync(long userId)
        {
            var userInputState = await _userService.GetUserInputStateAsync(userId);
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

        internal async Task DeleteUserDataAsync(long userId)
        {
            var userInputState = await _userService.GetUserInputStateAsync(userId);
            if (string.IsNullOrEmpty(userInputState.CreateInsuranceFlow.IdCacheKey) || string.IsNullOrEmpty(userInputState.CreateInsuranceFlow.DriverLicenseCacheKey))
            {
                return;
            }

            await _secretCache.DeleteAsync(userInputState.CreateInsuranceFlow.IdCacheKey);
            await _secretCache.DeleteAsync(userInputState.CreateInsuranceFlow.DriverLicenseCacheKey);
        }

        public async Task SetDocumentsForUser(long userId, IdDocument idDocument, DriverLicenseDocument licenseDocument)
        {
            var idKey = await _secretCache.StoreAsync(idDocument, TimeSpan.FromMinutes(30));
            var dlKey = await _secretCache.StoreAsync(licenseDocument, TimeSpan.FromMinutes(30));

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsDataConfirmationAwait, userId);
            await _userService.SetUserInputStateAsync(userId, uis =>
            {
                uis.CreateInsuranceFlow.IdCacheKey = idKey;
                uis.CreateInsuranceFlow.DriverLicenseCacheKey = dlKey;
            });

            await _botClient.SendMessage(userId, "Do you confirm data?", replyMarkup: AnswersData.DataConfirmationKeyboard);
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
