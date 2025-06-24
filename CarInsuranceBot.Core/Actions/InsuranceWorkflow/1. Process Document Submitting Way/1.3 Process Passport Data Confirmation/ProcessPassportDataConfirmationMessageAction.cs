using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Actions.InsuranceWorkflow;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions.PassportDataConfirmationAwait
{
    internal class ProcessPassportDataConfirmationMessageAction : ProcessConfirmationBaseMessageAction
    {
        public ProcessPassportDataConfirmationMessageAction(UserService userService, ITelegramBotClient botClient, OpenAIService openAiService) : base(userService, botClient, openAiService)
        {
        }

        protected override async Task OnConfirmation(Message update, CancellationToken cancellationToken)
        {
            if(update.From == null) { return; }

            await _botClient.SendMessage(
                update.Chat,
                await _openAiService.GetDiversifiedAnswer(AnswersData.SHARE_LICENSE_IN_CHAT_GPT_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD,
                cancellationToken: cancellationToken);

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.LicenseAwait, update.From.Id, cancellationToken: cancellationToken);
        }

        protected override async Task OnDecline(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null) { return; }

            await _botClient.SendMessage(
                update.Chat,
                await _openAiService.GetDiversifiedAnswer(AnswersData.DATA_DECLINED_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD,
                cancellationToken: cancellationToken);

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PassportAwait, update.From.Id, cancellationToken: cancellationToken);
        }

        protected override async Task OnNoAnswer(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null) { return; }

            await _botClient.SendMessage(
                update.Chat,
                AnswersData.NO_CONCRETE_ANSWER_FALLBACK_TEXT,
                replyMarkup: AnswersData.DATA_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);
        }
    }
}
