using CarInsuranceBot.Core.Abstractions;
using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.States.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static System.Formats.Asn1.AsnWriter;

namespace CarInsuranceBot.Core.Services
{
    /// <summary>
    /// Telegram bot <see cref="IUpdateHandler"/> implementation
    /// </summary>
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

        /// <summary>
        /// Update handling method
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Resolve target update type and run a task to not block update handling
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

            // Create service provider scope
            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    var sp = scope.ServiceProvider;
                    var userService = sp.GetRequiredService<UserService>();
                    var messageActionFactory = sp.GetRequiredService<ActionsFactory<Message>>();

                    // Get user state
                    UserState userState = await userService.GetUserStateByTelegramIdAsync(msg.From.Id, cancellationToken);

                    // Resolve target action based on current state
                    ActionBase<Message>? targetStateHandler = messageActionFactory.GetActionForState(userState);

                    if (targetStateHandler == null)
                    {
                        return;
                    }

                    //Execute action
                    await targetStateHandler.Execute(new MessageUpdateWrapper(msg), cancellationToken);

                } catch (Exception ex)
                {
                    _logger.LogError($"Error occured while processing message {msg.Id}, \n{ex.Message}, \n{ex.StackTrace}");
                }

            }

        }

        private async Task OnCallbackQuery(CallbackQuery callback, CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var sp = scope.ServiceProvider;
                    var userService = sp.GetRequiredService<UserService>();
                    var callbackQueryActionFactory = sp.GetRequiredService<ActionsFactory<CallbackQuery>>();

                    // Get user state
                    UserState userState = await userService.GetUserStateByTelegramIdAsync(callback.From.Id, cancellationToken);

                    // Resolve target action based on current state
                    ActionBase<CallbackQuery>? targetStateHandler = callbackQueryActionFactory.GetActionForState(userState);

                    if (targetStateHandler == null)
                    {
                        return;
                    }

                    //Execute action
                    await targetStateHandler.Execute(new CallbackQueryUpdateWrapper(callback), cancellationToken);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while processing callback query {callback.Id}, \n{ex.Message}, \n{ex.StackTrace}");
            }
            // Create service provider scope
        }

        private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            //Skip unknown update
            return Task.CompletedTask;
        }
    }
}
