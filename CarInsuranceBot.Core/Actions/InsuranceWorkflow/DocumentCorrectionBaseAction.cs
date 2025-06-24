using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Models.Documents;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.InsuranceWorkflow
{
    internal abstract class DocumentCorrectionBaseAction<TDocument> : MessageActionBase
    {
        protected readonly DocumentsService _documentsService;
        protected readonly OpenAIService _openAiService;
        protected readonly BotConfiguration _botConfig;
        protected readonly SecretCache _secretCache;

        public DocumentCorrectionBaseAction(
            UserService userService,
            ITelegramBotClient botClient,
            DocumentsService documentsService,
            OpenAIService openAiService,
            IOptions<BotConfiguration> botConfig,
            SecretCache secretCache) : base(userService, botClient)
        {
            _documentsService = documentsService;
            _openAiService = openAiService;
            _botConfig = botConfig.Value;
            _secretCache = secretCache;
        }

        protected override async Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null || string.IsNullOrEmpty(update.Text))
            {
                return;
            }

            var vehicleDocument = await RetreiveDocument(update, cancellationToken);
            if (vehicleDocument == null)
            {
                await OnNoDocuments(update, cancellationToken);
                return;
            }

            var invalidations = GetInvalidations(vehicleDocument);
            if (invalidations.Count == 0)
            {
                await OnNoInvalidations(update, vehicleDocument, cancellationToken);
                return;
            }

            var firstInvalidation = invalidations.First();
            var value = await _openAiService.GetValueFromInput<string>(firstInvalidation.Name, "string", update.Text, cancellationToken);
            if (!firstInvalidation.ValueHandler(value))
            {
                await _botClient.SendMessage(update.Chat, $"You didn't provide value for this field. \nYou need to fill up the \"{firstInvalidation.Name}\" field next");
            }

            var driverLicenseKey = await _secretCache.StoreAsync(vehicleDocument, TimeSpan.FromMinutes(30));
            await _userService.SetUserInputStateAsync(update.From.Id, uis =>
            {
                uis.CreateInsuranceFlow.DriverLicenseCacheKey = driverLicenseKey;
            }, cancellationToken);

            await _botClient.SendMessage(update.Chat, $"Field {firstInvalidation.Name} has been filled");
            invalidations = GetInvalidations(vehicleDocument);

            if (invalidations.Count == 0)
            {
                await OnNoInvalidations(update, vehicleDocument, cancellationToken);
                return;
            }

            firstInvalidation = invalidations.First();
            await _botClient.SendMessage(update.Chat, $"You need to fill up the \"{firstInvalidation.Name}\" field next");
        }

        protected abstract List<DocumentFieldModel<TDocument>> GetInvalidations(TDocument document);
        protected abstract Task OnNoDocuments(Message update, CancellationToken cancellationToken);
        protected abstract Task OnNoInvalidations(Message update, TDocument document, CancellationToken cancellationToken);
        protected abstract Task<TDocument?> RetreiveDocument(Message update, CancellationToken cancellationToken);
    }
}
