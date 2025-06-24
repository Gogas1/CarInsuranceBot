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

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions
{
    internal class ProcessPassportDataConfirmationCallbackAction : ProcessConfirmationBaseCallbackQueryAction
    {
        private readonly OpenAIService _openAiService;

        public ProcessPassportDataConfirmationCallbackAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAiService) : base(userService, botClient)
        {
            _openAiService = openAiService;
        }

        protected override async Task OnConfirmation(CallbackQuery update, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.SHARE_LICENSE_IN_CHAT_GPT_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD,
                cancellationToken: cancellationToken);

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.LicenseAwait, update.From.Id, cancellationToken: cancellationToken);
        }

        protected override async Task OnDecline(CallbackQuery update, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.DATA_DECLINED_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD,
                cancellationToken: cancellationToken);

            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PassportAwait, update.From.Id, cancellationToken: cancellationToken);
        }
    }
}
