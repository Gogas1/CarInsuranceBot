using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Services;
using CarInsuranceBot.Core.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Abstractions;

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    internal abstract class MessageActionBase : BusyHandlingActionBase<Message>
    {
        protected override TimeSpan Timeout => TimeSpan.FromSeconds(20);
        
        protected MessageActionBase(UserService userService, ITelegramBotClient botClient) : base(botClient, userService)
        {
        }

        public sealed override async Task Execute(UpdateWrapperBase<Message> update, CancellationToken cancellationToken)
        {
            if(update.GetUser() is User user)
            {
                await _botClient.SendChatAction(user.Id, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: cancellationToken);
            }

            await base.Execute(update, cancellationToken);
        }

        protected override async Task OnTimeoutAsync(Message update)
        {
            if(update.From == null)
            {
                return;
            }

            await _botClient.SendMessage(update.From.Id, AnswersData.TIMEOUT_ANSWER_TEXT);
        }
    }
}
