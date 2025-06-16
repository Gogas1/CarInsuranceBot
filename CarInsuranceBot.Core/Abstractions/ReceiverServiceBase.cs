using CarInsuranceBot.Core.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace CarInsuranceBot.Core.Abstractions
{
    /// <summary>
    /// <see cref="IReceiverService"/> implementation for handeling receiving updates by <see cref="ITelegramBotClient"/>
    /// </summary>
    /// <typeparam name="TUpdateHandler">Concrete telegram bot update handler type that implements <see cref="IUpdateHandler"/></typeparam>
    internal class ReceiverServiceBase<TUpdateHandler> : IReceiverService where TUpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly TUpdateHandler _updateHandler;

        public ReceiverServiceBase(ITelegramBotClient botClient, TUpdateHandler updateHandler)
        {
            _botClient = botClient;
            _updateHandler = updateHandler;
        }

        public async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            //Set up receiver options
            var receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = [],
                DropPendingUpdates = true,
            };

            //Start receiving updates
            await _botClient.ReceiveAsync(_updateHandler, receiverOptions, cancellationToken);
        }
    }
}
