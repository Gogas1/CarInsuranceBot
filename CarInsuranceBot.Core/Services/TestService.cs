using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Models.Documents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Services
{
    public class TestService
    {
        private readonly UserService _userService;
        private readonly SecretCache _secretCache;
        private readonly DocumentsService _documentsService;
        private readonly ITelegramBotClient _telegramBotClient;


        public TestService(IServiceProvider serviceProvider)
        {
            _userService = serviceProvider.GetRequiredService<UserService>();
            _documentsService = serviceProvider.GetRequiredService<DocumentsService>();
            _telegramBotClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
            _secretCache = serviceProvider.GetRequiredService<SecretCache>();
        }

        public async Task GotoVehicleDataCorrectingState(long userId)
        {
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.LicenseDataCorrectionAwait, userId, default);

            var data = await _documentsService.GetDataForUserAsync(userId, default);

            if (data == null)
            {
                return;
            }

            var vehicleDocument = data.DriverLicenseDocument;

            var invalidations = vehicleDocument.GetInvalidFieldsHandlers();
            var firstInvalidation = invalidations.First();
            await _telegramBotClient.SendMessage(userId, $"You need to fill up the \"{firstInvalidation.Name}\" field next");
        }

        public async Task AddVehicleDocument(long userId, DriverLicenseDocument vehicleDocument)
        {
            var vehicleDocumentKey = await _secretCache.StoreAsync(vehicleDocument, TimeSpan.FromMinutes(30));
            var idKey = await _secretCache.StoreAsync(new IdDocument(), TimeSpan.FromMinutes(30));

            await _userService.SetUserInputStateAsync(userId, uis =>
            {
                uis.CreateInsuranceFlow.IdCacheKey = idKey;
                uis.CreateInsuranceFlow.DriverLicenseCacheKey = vehicleDocumentKey;
            }, default);
        }
    }
}
