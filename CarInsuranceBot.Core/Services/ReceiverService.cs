using CarInsuranceBot.Core.Abstractions;
using Telegram.Bot;

namespace CarInsuranceBot.Core.Services
{
    internal class ReceiverService : ReceiverServiceBase<UpdateService>
    {
        public ReceiverService(ITelegramBotClient botClient, UpdateService updateService) : base(botClient, updateService)
        {

        }
    }
}
