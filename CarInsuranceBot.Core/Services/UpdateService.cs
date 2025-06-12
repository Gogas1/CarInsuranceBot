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

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await(update switch
            {
                { Message: { } message } => OnMessage(message),
                { CallbackQuery: { } callback } => OnCallbackQuery(callback),
                _ => UnknownUpdateHandlerAsync(update)
            });
        }

        private async Task OnMessage(Message msg)
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

                UserState userState = await userService.GetUserStateByTelegramIdAsync(msg.From.Id);

                ActionBase<Message>? targetStateHandler = messageActionFactory.GetActionForState(userState);

                if (targetStateHandler == null)
                {
                    return;
                }

                await targetStateHandler.Execute(msg);

            }

        }

        private async Task OnCallbackQuery(CallbackQuery callback)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var userService = sp.GetRequiredService<UserService>();
                var callbackQueryActionFactory = sp.GetRequiredService<ActionsFactory<CallbackQuery>>();

                UserState userState = await userService.GetUserStateByTelegramIdAsync(callback.From.Id);

                ActionBase<CallbackQuery>? targetStateHandler = callbackQueryActionFactory.GetActionForState(userState);

                if (targetStateHandler == null)
                {
                    return;
                }

                await targetStateHandler.Execute(callback);

            }
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            return Task.CompletedTask;
        }
    }
}
