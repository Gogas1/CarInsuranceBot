using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Actions.DefaultActions;
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

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions
{
    internal abstract class ProcessDataCorrectionCallbackQueryAction : DefaultCallbackQueryAction
    {
        protected OpenAIService _openAiService;

        protected abstract UserState ExitState { get; }

        public ProcessDataCorrectionCallbackQueryAction(UserService userService, ITelegramBotClient botClient, OpenAIService openAiService) : base(userService, botClient)
        {
            _openAiService = openAiService;
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            switch(update.Data)
            {
                case AnswersData.RESUBMIT_PHOTO_BUTTON_DATA:
                    await _botClient.SendMessage(
                        update.From.Id,
                        await GetMessageAsync(cancellationToken),
                        replyMarkup: AnswersData.STOP_WORKFLOW_KEYBOARD,
                        cancellationToken: cancellationToken);
                    await _userService.SetUserStateByTelegramIdAsync(ExitState, update.From.Id, cancellationToken);
                    return;
            }

            await base.ProcessLogicAsync(update, cancellationToken);
        }

        protected abstract Task<string> GetMessageAsync(CancellationToken cancellationToken);
    }
}
