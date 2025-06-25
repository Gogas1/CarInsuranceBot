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

namespace CarInsuranceBot.Core.Actions.Abstractions
{
    /// <summary>
    /// Base for actions, processing a document from a chat photo
    /// </summary>
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

        /// <summary>
        /// Processes the document using Mindee API and forms the document model
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TMindeeDocument"></typeparam>
        /// <param name="photoStream">Image stream</param>
        /// <param name="userId">Telegram user Id</param>
        /// <param name="fileName">File name for the Mindee API</param>
        /// <param name="mappingFunction">Mapping function from the Mindee document to the application document model</param>
        /// <param name="onErrorStatus">API error handler</param>
        /// <param name="onInvalidation"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<TDocument?> ProcessDocumentAsync<TDocument, TMindeeDocument>(
            Stream photoStream,
            long userId,
            string fileName,
            Func<TMindeeDocument, TDocument?> mappingFunction,
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

                // Return mapped document
                return doc;
            }
            catch (HttpRequestException ex)
            {
                // Handle error
                if (ex.StatusCode != null)
                {
                    onErrorStatus((int)ex.StatusCode);
                }

                return null;
            }
            catch (Mindee400Exception)
            {
               onErrorStatus(400);

                return null;
            }
        }

        /// <summary>
        /// Handle no submitted photo
        /// </summary>
        /// <param name="update"></param>
        /// <param name="guidance"></param>
        /// <param name="noPhotoMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Handle answer on no photo
        /// </summary>
        /// <param name="update"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task OnNoPhoto(Message update, string message, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(update.Chat, message, replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD, cancellationToken: cancellationToken);
        }
    }
}
