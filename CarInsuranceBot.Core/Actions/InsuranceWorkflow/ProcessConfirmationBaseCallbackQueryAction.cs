using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
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
    internal abstract class ProcessConfirmationBaseCallbackQueryAction : CallbackQueryActionBase
    {
        public ProcessConfirmationBaseCallbackQueryAction(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            var data = update.Data;
            if (data == null)
            {
                return;
            }

            switch (data)
            {
                case AnswersData.DATA_CONFIRMATION_BUTTON_DATA:
                    await OnConfirmation(update, cancellationToken);
                    break;
                case AnswersData.DATA_DECLINE_BUTTON_DATA:
                    await OnDecline(update, cancellationToken);
                    break;
                case AnswersData.STOP_WORKFLOW_BUTTON_DATA:
                   await OnWorkflowStop(update, cancellationToken);
                   break;
            }
        }

        private async Task OnWorkflowStop(CallbackQuery update, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
            update.From.Id,
            AnswersData.USER_RECONSIDERED_ANSWER_TEXT,
            replyMarkup: AnswersData.HOME_KEYBOARD,
            cancellationToken: cancellationToken);
            await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id, cancellationToken);
        }

        protected abstract Task OnConfirmation(CallbackQuery update, CancellationToken cancellationToken);
        protected abstract Task OnDecline(CallbackQuery update, CancellationToken cancellationToken);
    }
}
