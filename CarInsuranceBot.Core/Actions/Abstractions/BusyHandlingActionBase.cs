using CarInsuranceBot.Core.Abstractions;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.Abstractions
{
    /// <summary>
    /// Abstract <see cref="ActionBase{TUpdateType}"/> implementation. Provides common functionality for all actions: timeout logic, bot busyness for specific user
    /// </summary>
    /// <typeparam name="TUpdateType"><inheritdoc/></typeparam>
    internal abstract class BusyHandlingActionBase<TUpdateType> : ActionBase<TUpdateType>
    {
        /// <summary>
        /// Update processing timeout getter
        /// </summary>
        protected abstract TimeSpan Timeout { get; }

        /// <summary>
        /// Separate <see cref="CancellationTokenSource"/> to handle action execution flow cancelling in case of bot busyness for specific user
        /// </summary>
        protected readonly CancellationTokenSource _busynessCancellationTokenSource;
        protected readonly ITelegramBotClient _botClient;
        protected readonly UserService _userService;

        public BusyHandlingActionBase(ITelegramBotClient botClient, UserService userService)
        {
            _busynessCancellationTokenSource = new CancellationTokenSource();
            _botClient = botClient;
            _userService = userService;
        }

        /// <summary>
        /// Implemented action execution method
        /// </summary>
        /// <param name="update"><inheritdoc/></param>
        /// <param name="cancellationToken"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override async Task Execute(UpdateWrapperBase<TUpdateType> update, CancellationToken cancellationToken)
        {
            // Create CTS to handle timeout and link it with passed external cancellation token
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // Set token source timeout
            cts.CancelAfter(Timeout);
            var token = cts.Token;

            // Validates if bot already processing update for this user(bot is busy). If so, send message and cancel _busynessCancellationTokenSource
            await ValidateUserBusyness(update.GetUser(), token);
            // If bot is busy or timeout before actual action logic - return
            if (token.IsCancellationRequested || _busynessCancellationTokenSource.IsCancellationRequested)
                return;

            // Set bot busyness for this user
            await EnterBusynessScope(update.GetUser()).ConfigureAwait(false);
            try
            {
                // Process implemented action logic
                await ProcessLogicAsync(update.Update, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // If timeout or canceled during actual action logic - handle it
                await OnTimeoutAsync(update.Update);
            }
            finally
            {
                // Remove bot busyness for this user
                await ExitBusynessScope(update.GetUser()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get bot busyness for user
        /// </summary>
        /// <param name="user">User that initiated update</param>
        /// <returns>Is bot busy processing action for this user</returns>
        protected bool IsUserBusy(User? user)
        {
            if (user == null)
            {
                return false;
            }

            // Get cached busyness flag for this user
            return _userService.IsUserBusy(user.Id);
        }

        /// <summary>
        /// Sets flag indicating bot is busy for this user
        /// </summary>
        /// <param name="user">User that initiated update</param>
        /// <returns><see cref="Task"/></returns>
        protected Task EnterBusynessScope(User? user)
        {
            if (user != null)
            {
                // Set busyness flag for this user
                _userService.SetUserBusy(user.Id, true);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes flag indicating bot is busy for this user
        /// </summary>
        /// <param name="user">User that initiated update</param>
        /// <returns><see cref="Task"/></returns>
        protected Task ExitBusynessScope(User? user)
        {
            if (user != null)
            {
                _userService.SetUserBusy(user.Id, false);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates if bot is busy for this user
        /// </summary>
        /// <param name="user">User that initiated update</param>
        /// <param name="timeoutToken">Timeout token</param>
        /// <returns><see cref="Task"/></returns>
        private async Task ValidateUserBusyness(User? user, CancellationToken timeoutToken)
        {
            // If busy
            if (IsUserBusy(user))
            {
                // Cancel busyness CTS
                _busynessCancellationTokenSource.Cancel();
                // Process busyness logic for this user
                await ProcessUserIsBusyAsync(user, timeoutToken);
            }
        }

        /// <summary>
        /// Processes logic if bot is busy for this user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="timeoutToken"></param>
        /// <returns></returns>
        protected async Task ProcessUserIsBusyAsync(User? user, CancellationToken timeoutToken)
        {
            if (user == null)
            {
                return;
            }

            // Notify user about bot busyness
            await _botClient.SendMessage(user.Id, AnswersData.BOT_BUSY_FOR_USER_ANSWER_TEXT, cancellationToken: timeoutToken);
        }

        /// <summary>
        /// Abstract method to implement actual action logic
        /// </summary>
        /// <param name="update">Concrete telegram update type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="Task"/></returns>
        protected abstract Task ProcessLogicAsync(TUpdateType update, CancellationToken cancellationToken);

        /// <summary>
        /// Abstract method to implement logic in case of timeout
        /// </summary>
        /// <param name="update">Concrete telegram update type</param>
        /// <returns><see cref="Task"/></returns>
        protected abstract Task OnTimeoutAsync(TUpdateType update);
    }
}
