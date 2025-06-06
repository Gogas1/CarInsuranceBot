using CarInsuranceBot.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
