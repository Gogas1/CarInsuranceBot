using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.Core.Services;
using Mindee;
using Mindee.Exceptions;
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
    internal abstract class ChatDocumentProcessingAction : GeneralInformationalMessageAction
    {
        private readonly MindeeClient _mindeeClient;

        protected ChatDocumentProcessingAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAiService,
            MindeeClient mindeeClient) : base(userService, botClient, openAiService)
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

            try
            {
                // Extract data using Mindee
                var response = await _mindeeClient.EnqueueAndParseAsync<TMindeeDocument>(
                    new LocalInputSource(photoStream, fileName));
                var doc = mappingFunction(response.Document.Inference);
                cancellationToken.ThrowIfCancellationRequested();

                if (response.ApiRequest.StatusCode != 200)
                {
                    onErrorStatus(response.ApiRequest.StatusCode);
                    return null;
                }

                // Handle invalidation
                if (!validationFunction(doc))
                {
                    onInvalidation(doc);
                    return null;
                }

                // Return mapped document
                return doc;
            }
            catch (HttpRequestException ex)
            {
                // Handle error
                if(ex.StatusCode != null)
                {
                    onErrorStatus((int)ex.StatusCode);
                }

                return null;
            }
        }

        protected async Task ProcessNoPhoto(Message update, string guidance, string noPhotoMessage, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                var defaultOption = new OpenAIService.SelectItem(-1, "No question", async _ => await OnNoPhoto(update, noPhotoMessage, cancellationToken));
                var questions = CreateBaseQuestionsOptionsList(update, guidance, cancellationToken: cancellationToken, replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD);
                var selectedOption = await _openAiService.GetSelectionByTextAsync(questions, defaultOption, update.Text.Truncate(100), cancellationToken);

                if (selectedOption == null)
                {
                    defaultOption.OnSelection();
                    return;
                }

                selectedOption.OnSelection();
                return;
            }

            await OnNoPhoto(update, noPhotoMessage, cancellationToken);
        }

        protected async Task OnNoPhoto(Message update, string message, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(update.Chat, message, replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD, cancellationToken: cancellationToken);
        }

        protected async Task OnExtractionError(Message update, string message, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                update.Chat,
                message,
                replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD,
                cancellationToken: cancellationToken);
        }
    }
}
