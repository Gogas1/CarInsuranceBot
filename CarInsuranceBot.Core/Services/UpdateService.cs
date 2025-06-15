using CarInsuranceBot.Core.Abstractions;
using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.States.Abstractions;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ILogger<UpdateService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public UpdateService(
            ILogger<UpdateService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            _logger.LogError($"{exception.Message}, {exception.StackTrace}");
            return Task.CompletedTask;
        }

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = Task.Run(async () =>
            {
                switch (update)
                {
                    case { Message: { } message }:
                        await OnMessage(message, cancellationToken);
                        break;
                    case { CallbackQuery: { } callback }:
                        await OnCallbackQuery(callback, cancellationToken);
                        break;
                    default:
                        await UnknownUpdateHandlerAsync(update, cancellationToken);
                        break;
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        private async Task OnMessage(Message msg, CancellationToken cancellationToken)
        {
            if (msg.From == null)
            {
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var userService = sp.GetRequiredService<UserService>();
                var messageActionFactory = sp.GetRequiredService<ActionsFactory<Message>>();

                UserState userState = await userService.GetUserStateByTelegramIdAsync(msg.From.Id, cancellationToken);

                ActionBase<Message>? targetStateHandler = messageActionFactory.GetActionForState(userState);

                if (targetStateHandler == null)
                {
                    return;
                }

                await targetStateHandler.Execute(new MessageUpdateWrapper(msg), cancellationToken);

            }

        }

        private async Task OnCallbackQuery(CallbackQuery callback, CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var userService = sp.GetRequiredService<UserService>();
                var callbackQueryActionFactory = sp.GetRequiredService<ActionsFactory<CallbackQuery>>();

                UserState userState = await userService.GetUserStateByTelegramIdAsync(callback.From.Id, cancellationToken);

                ActionBase<CallbackQuery>? targetStateHandler = callbackQueryActionFactory.GetActionForState(userState);

                if (targetStateHandler == null)
                {
                    return;
                }

                await targetStateHandler.Execute(new CallbackQueryUpdateWrapper(callback), cancellationToken);

            }
        }

        private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
