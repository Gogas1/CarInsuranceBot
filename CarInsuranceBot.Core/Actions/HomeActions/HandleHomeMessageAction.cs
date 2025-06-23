using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    /// <summary>
    /// <see cref="MessageActionBase"/> implementation to handle home state of the bot.
    /// </summary>
    internal class HandleHomeMessageAction : GeneralInformationalMessageAction
    {
        private readonly BotConfiguration _botConfig;
        private readonly DocumentsService _documentsService;

        public HandleHomeMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAiService,
            IOptions<BotConfiguration> botConfig,
            DocumentsService documentsService) : base(userService, botClient, openAiService)
        {
            _botConfig = botConfig.Value;
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
            OpenAIService.SelectItem defaultOption = new OpenAIService.SelectItem(
                -1,
                "Get bot options, start conversation with a bot",
                async _ => await OnHomeOption(update, cancellationToken));

            //Init options list
            List<OpenAIService.SelectItem> options = [
                defaultOption,
                new OpenAIService.SelectItem(
                    0, 
                    AnswersData.GET_INSURANCE_BUTTON_TEXT,
                    async _ => await OnInsuranceOption(update, cancellationToken))
                ];

            options.AddRange(CreateBaseQuestionsOptionsList(
                update,
                AnswersData.HOME_STATE_GUIDANCE,
                AnswersData.HOME_KEYBOARD,
                cancellationToken));

            // If user wrote something
            if (update.Text != null)
            {
                // Get selected option by GPT and execute
                selectedOption = await _openAiService.GetSelectionByTextAsync(options, defaultOption, update.Text.Truncate(100), cancellationToken);
                selectedOption.OnSelection();

                return;
            }

            // Otherwise default option
            defaultOption.OnSelection();
        }

        private async Task OnHomeOption(Message update, CancellationToken cancellationToken)
        {
            // Send home message and options
            await _botClient.SendMessage(
                        update.Chat,
                        await _openAiService.GetDiversifiedAnswer(AnswersData.HOME_MESSAGE_SETTINGS, cancellationToken),
                        replyMarkup: AnswersData.HOME_KEYBOARD,
                        cancellationToken: cancellationToken);
        }

        private async Task OnInsuranceOption(Message update, CancellationToken cancellationToken)
        {
            if(update.From == null)
            {
                return;
            }

            var nonce = _documentsService.SetNonceForUser(update.From.Id);

            // Send message about submitting the documents and authorization keyboard
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.START_INSURANCE_WORKFLOW_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfig, nonce));

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(UserState.DocumentsAwait, update.From.Id, cancellationToken);
        }
    }
}
