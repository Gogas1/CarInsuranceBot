using CarInsuranceBot.Core.Abstractions;
using Telegram.Bot;

namespace CarInsuranceBot.Core.Services
{
    /// <summary>
    /// <see cref="ReceiverServiceBase{TUpdateHandler}"/> implementation using <see cref="UpdateService"/>
    /// </summary>
    internal class ReceiverService : ReceiverServiceBase<UpdateService>
    {
        public ReceiverService(ITelegramBotClient botClient, UpdateService updateService) : base(botClient, updateService)
        {

        }
    }
}
