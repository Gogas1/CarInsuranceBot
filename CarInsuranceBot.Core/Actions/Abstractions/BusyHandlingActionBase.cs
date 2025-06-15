using CarInsuranceBot.Core.Abstractions;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CarInsuranceBot.Core.Actions.Abstractions
{
    internal abstract class BusyHandlingActionBase<T> : ActionBase<T>
    {
        protected abstract TimeSpan Timeout { get; }

        protected readonly CancellationTokenSource _busynessCancellationTokenSource;
        protected readonly ITelegramBotClient _botClient;
        protected readonly UserService _userService;

        public BusyHandlingActionBase(ITelegramBotClient botClient, UserService userService)
        {
            _busynessCancellationTokenSource = new CancellationTokenSource();
            _botClient = botClient;
            _userService = userService;
        }

        public override async Task Execute(UpdateWrapperBase<T> update, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Timeout);
            var token = cts.Token;

            await ValidateUserBusyness(update.GetUser(), token);
            if (token.IsCancellationRequested || _busynessCancellationTokenSource.IsCancellationRequested)
                return;

            await EnterBusynessScope(update.GetUser()).ConfigureAwait(false);
            try
            {
                await ProcessLogicAsync(update.Update, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                await OnTimeoutAsync(update.Update);
            }
            finally
            {
                await ExitBusynessScope(update.GetUser()).ConfigureAwait(false);
            }
        }

        protected bool IsUserBusy(User? user)
        {
            if (user == null)
            {
                return false;
            }

            return _userService.IsUserBusy(user.Id);
        }
        protected Task EnterBusynessScope(User? user)
        {
            if (user != null)
            {
                _userService.SetUserBusy(user.Id, true);
            }

            return Task.CompletedTask;
        }
        protected Task ExitBusynessScope(User? user)
        {
            if (user != null)
            {
                _userService.SetUserBusy(user.Id, false);
            }

            return Task.CompletedTask;
        }
        protected async Task ProcessUserIsBusyAsync(User? user, CancellationToken timeoutToken)
        {
            if (user == null)
            {
                return;
            }

            await _botClient.SendMessage(user.Id, AnswersData.BOT_BUSY_FOR_USER_ANSWER_TEXT, cancellationToken: timeoutToken);
        }
        private async Task ValidateUserBusyness(User? user, CancellationToken timeoutToken)
        {
            if (IsUserBusy(user))
            {
                _busynessCancellationTokenSource.Cancel();
                await ProcessUserIsBusyAsync(user, timeoutToken);
            }
        }

        protected abstract Task ProcessLogicAsync(T update, CancellationToken cancellationToken);
        protected abstract Task OnTimeoutAsync(T update);
    }
}
