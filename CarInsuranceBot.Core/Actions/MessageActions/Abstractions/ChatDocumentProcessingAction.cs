using CarInsuranceBot.Core.Services;
using Mindee;
using Mindee.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Passport;

namespace CarInsuranceBot.Core.Actions.MessageActions.Abstractions
{
    internal abstract class ChatDocumentProcessingAction : MessageActionBase
    {
        private readonly MindeeClient _mindeeClient;

        public ChatDocumentProcessingAction(
            UserService userService,
            ITelegramBotClient botClient,
            MindeeClient mindeeClient) : base(userService, botClient)
        {
            _mindeeClient = mindeeClient;
        }

        protected async Task<TDocument?> ProcessDocumentAsync<TDocument, TMindeeDocument>(
            Stream photoStream,
            long userId,
            string fileName,
            Func<TMindeeDocument, TDocument?> mappingFunction,
            Func<TDocument?, bool> validationFunction,
            Action<int> onErrorStatus,
            Action<TDocument?> onInvalidation,
            CancellationToken cancellationToken) where TMindeeDocument : class, new() where TDocument : class, new()
        {
            // Extract data using Mindee
            var response = await _mindeeClient.EnqueueAndParseAsync<TMindeeDocument>(
                new LocalInputSource(photoStream, fileName));
            cancellationToken.ThrowIfCancellationRequested();

            // Handle error
            if (response.ApiRequest.StatusCode != 200)
            {
                onErrorStatus(response.ApiRequest.StatusCode);
                return null;
            }

            var doc = mappingFunction(response.Document.Inference);

            // Handle invalidation
            if (!validationFunction(doc))
            {
                onInvalidation(doc);
                return null;
            }

            // Return mapped document
            return doc;
        }
    }
}
