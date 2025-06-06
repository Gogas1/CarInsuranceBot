using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.States.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Services
{
    internal class UpdateService : IUpdateHandler
    {
        private readonly UserService _userService;
        private readonly ActionsFactory<Message> _messageActionFactory;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(ActionsFactory<Message> messageActionFactory, ILogger<UpdateService> logger, UserService userService)
        {
            _messageActionFactory = messageActionFactory;
            _logger = logger;
            _userService = userService;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            _logger.LogError(exception.Message);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await(update switch
            {
                { Message: { } message } => OnMessage(message),
                _ => UnknownUpdateHandlerAsync(update)
            });
        }

        private async Task OnMessage(Message msg)
        {
            if (msg.From == null)
            {
                return;
            }

            UserState userState = await _userService.GetUserStateByTelegramIdAsync(msg.From.Id);

            ActionBase<Message>? targetStateHandler = _messageActionFactory.GetActionForState(userState);

            if (targetStateHandler == null)
            {
                return;
            }

            await targetStateHandler.Execute(msg);
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            return Task.CompletedTask;
        }
    }
}
