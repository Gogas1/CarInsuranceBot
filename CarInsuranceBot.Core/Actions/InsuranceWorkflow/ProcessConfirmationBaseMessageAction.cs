using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.InsuranceWorkflow
{
    internal abstract class ProcessConfirmationBaseMessageAction : GeneralInformationalMessageAction
    {
        protected ProcessConfirmationBaseMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAiService) : base(userService, botClient, openAiService)
        {
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

        protected abstract Task OnNoAnswer(Message update, CancellationToken cancellationToken);
        protected abstract Task OnConfirmation(Message update, CancellationToken cancellationToken);
        protected abstract Task OnDecline(Message update, CancellationToken cancellationToken);
    }
}
