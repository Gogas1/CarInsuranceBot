using CarInsuranceBot.Core.Abstractions;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.Abstractions
{
    /// <summary>
    /// <see cref="BusyHandlingActionBase{TUpdateType}"/> implementation for <see cref="CallbackQuery"/> update type
    /// </summary>
    internal abstract class CallbackQueryActionBase : BusyHandlingActionBase<CallbackQuery>
    {
        protected override TimeSpan Timeout => TimeSpan.FromSeconds(15);

        protected CallbackQueryActionBase(UserService userService, ITelegramBotClient botClient) : base(botClient, userService)
        {
        }

        /// <summary>
        /// <inheritdoc/>. Sealed to prevent further override. To implement action logic override <see cref="BusyHandlingActionBase{TUpdateType}.ProcessLogicAsync(TUpdateType, CancellationToken)"/>
        /// </summary>
        /// <param name="update"><inheritdoc/></param>
        /// <param name="cancellationToken"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public sealed override async Task Execute(UpdateWrapperBase<CallbackQuery> update, CancellationToken cancellationToken)
        {
            if (update.GetUser() is User user)
            {
                // Send typing chat action to show processing visibility
                await _botClient.SendChatAction(user.Id, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken: cancellationToken);
                // Answer callback query
                await _botClient.AnswerCallbackQuery(update.Update.Id);
            }
            
            await base.Execute(update, cancellationToken);
        }

        /// <summary>
        /// Process timeout logic for <see cref="CallbackQuery"/> update type
        /// </summary>
        /// <param name="update"><see cref="CallbackQuery"/> instance</param>
        /// <returns><inheritdoc/></returns>
        protected override async Task OnTimeoutAsync(CallbackQuery update)
        {
            if (update.From == null)
            {
                return;
            }

            // Answer callback query
            await _botClient.AnswerCallbackQuery(update.Id);
            // Notify user
            await _botClient.SendMessage(update.From.Id, AnswersData.TIMEOUT_ANSWER_TEXT);
        }
    }
}
