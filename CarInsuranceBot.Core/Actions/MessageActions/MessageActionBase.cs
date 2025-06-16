using CarInsuranceBot.Core.Abstractions;
using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    /// <summary>
    /// <see cref="BusyHandlingActionBase{TUpdateType}"/> implementation for <see cref="Message"/> update type
    /// </summary>
    internal abstract class MessageActionBase : BusyHandlingActionBase<Message>
    {
        protected override TimeSpan Timeout => TimeSpan.FromSeconds(20);
        
        protected MessageActionBase(UserService userService, ITelegramBotClient botClient) : base(botClient, userService)
        {
        }

        /// <summary>
        /// <inheritdoc/>. Sealed to prevent further override. To implement action logic override <see cref="BusyHandlingActionBase{TUpdateType}.ProcessLogicAsync(TUpdateType, CancellationToken)"/>
        /// </summary>
        /// <param name="update"><inheritdoc/></param>
        /// <param name="cancellationToken"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public sealed override async Task Execute(UpdateWrapperBase<Message> update, CancellationToken cancellationToken)
        {
            if (update.GetUser() is User user)
            {
                // Send typing chat action to show processing visibility
                await _botClient.SendChatAction(user.Id, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: cancellationToken);
            }

            await base.Execute(update, cancellationToken);
        }

        /// <summary>
        /// Process timeout logic for <see cref="Message"/> update type
        /// </summary>
        /// <param name="update"><see cref="Message"/> instance</param>
        /// <returns><inheritdoc/></returns>
        protected override async Task OnTimeoutAsync(Message update)
        {
            if (update.From == null)
            {
                return;
            }

            // Notify user
            await _botClient.SendMessage(update.From.Id, AnswersData.TIMEOUT_ANSWER_TEXT);
        }
    }
}
