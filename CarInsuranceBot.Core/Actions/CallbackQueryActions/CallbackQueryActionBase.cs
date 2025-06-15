using CarInsuranceBot.Core.Abstractions;
using CarInsuranceBot.Core.Actions.Abstractions;
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
    internal abstract class CallbackQueryActionBase : BusyHandlingActionBase<CallbackQuery>
    {
        protected override TimeSpan Timeout => TimeSpan.FromSeconds(15);

        protected CallbackQueryActionBase(UserService userService, ITelegramBotClient botClient) : base(botClient, userService)
        {
        }

        public sealed override async Task Execute(UpdateWrapperBase<CallbackQuery> update, CancellationToken cancellationToken)
        {
            if (update.GetUser() is User user)
            {
                await _botClient.SendChatAction(user.Id, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: cancellationToken);
            }

            await base.Execute(update, cancellationToken);
        }

        protected override async Task OnTimeoutAsync(CallbackQuery update)
        {
            if (update.From == null)
            {
                return;
            }

            await _botClient.AnswerCallbackQuery(update.Id);
            await _botClient.SendMessage(update.From.Id, AnswersData.TIMEOUT_ANSWER_TEXT);
        }
    }
}
