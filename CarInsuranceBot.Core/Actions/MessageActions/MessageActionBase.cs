using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Services;
using CarInsuranceBot.Core.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using CarInsuranceBot.Core.Constants;

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    internal class MessageActionBase : ActionBase<Message>
    {
        private readonly CancellationTokenSource _executionCancellationTokenSource;
        protected readonly ITelegramBotClient _botClient;
        protected readonly UserService _userService;

        protected bool IsCancellationRequested
        {
            get
            {
                return _executionCancellationTokenSource.IsCancellationRequested;
            }
        }

        public MessageActionBase(UserService userService, ITelegramBotClient botClient)
        {
            _userService = userService;
            _botClient = botClient;
            _executionCancellationTokenSource = new CancellationTokenSource();
        }

        public override async Task Execute(Message update)
        {
            await ProcessReset(update);
        }

        protected virtual async Task ProcessReset(Message update)
        {
            if (update.From == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(update.Text))
            {
                return;
            }

            var parts = update.Text.Trim().Split(' ');

            if (parts.Length < 1)
            {
                return;
            }

            if (parts[0] == "/reset")
            {
                await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id);
                if(!string.IsNullOrEmpty(AnswersData.ResetMessage))
                {
                    await _botClient.SendMessage(update.From.Id, AnswersData.ResetMessage);
                }
            }
        }
    }
}
