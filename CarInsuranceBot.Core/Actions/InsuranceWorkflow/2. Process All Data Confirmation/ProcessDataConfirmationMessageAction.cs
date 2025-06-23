using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    internal class ProcessDataConfirmationMessageAction : GeneralInformationalMessageAction
    {        
        private readonly DocumentsService _documentsService;
        private readonly BotConfiguration _botConfiguration;

        public ProcessDataConfirmationMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAIService,
            DocumentsService documentsService,
            IOptions<BotConfiguration> botConfiguration) : base(userService, botClient, openAIService)
        {
            _openAiService = openAIService;
            _documentsService = documentsService;
            _botConfiguration = botConfiguration.Value;
        }

        protected override async Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            OpenAIService.SelectItem? selectedOption = null;
            // Init default options
            OpenAIService.SelectItem defautOption = new OpenAIService.SelectItem(
                -1,
                "Ambiguous answer",
                async _ => await OnNoAnswer(update, cancellationToken));

            //Init options list
            List<OpenAIService.SelectItem> options = [
                new OpenAIService.SelectItem(
                    0,
                    "Yes, I confirm data is correct",
                    async _ => await OnConfirmation(update, cancellationToken)),
                new OpenAIService.SelectItem(
                    1,
                    "No, data is incorrect",
                    async _ => await OnDecline(update, cancellationToken))
                ];

            if (update.Text != null)
            {
                options.AddRange(CreateBaseQuestionsOptionsList(
                    update,
                    AnswersData.DATA_CONFIRMATION_AWAIT_STATE_GUIDANCE,
                    AnswersData.DATA_CONFIRMATION_KEYBOARD,
                    cancellationToken));
                // Get selected option by GPT and execute
                selectedOption = await _openAiService.GetSelectionByTextAsync(options, defautOption, update.Text.Truncate(100), cancellationToken);
                selectedOption.OnSelection();

                return;
            }

            defautOption.OnSelection();
        }

        private async Task OnNoAnswer(Message update, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                update.Chat,
                AnswersData.NO_CONCRETE_ANSWER_FALLBACK_TEXT,
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);
        }

        private async Task OnConfirmation(Message update, CancellationToken cancellationToken)
        {
            if(update.From == null)
            {
                return;
            }

            // Send message about the next step - price agreement text and keyboard
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.DATA_CONFIRMED_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.PRICE_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);
            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PriceConfirmationAwait, update.From.Id, cancellationToken);
            return;
        }

        private async Task OnDecline(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            var newNonce = _documentsService.SetNonceForUser(update.From.Id);

            // Send message about resubmitting the documents and authorization keyboard
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.DATA_DECLINED_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfiguration, newNonce));
            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, update.From.Id, cancellationToken);
            return;
        }
    }
}
