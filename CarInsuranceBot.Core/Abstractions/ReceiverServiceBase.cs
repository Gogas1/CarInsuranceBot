using CarInsuranceBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace CarInsuranceBot.Core.Abstractions
{
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
            var receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = [],
                DropPendingUpdates = false,
            };

            await _botClient.ReceiveAsync(_updateHandler, receiverOptions, cancellationToken);
        }
    }
}
