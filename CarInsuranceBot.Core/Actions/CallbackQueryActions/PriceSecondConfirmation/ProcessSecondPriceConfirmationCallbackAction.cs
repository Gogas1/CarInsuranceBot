using CarInsuranceBot.Core.Actions.CallbackQueryActions.PriceConfirmationAwait;
using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.PriceSecondConfirmation
{
    internal class ProcessSecondPriceConfirmationCallbackAction : ProcessPriceConfirmationCallbackAction
    {
        public ProcessSecondPriceConfirmationCallbackAction(UserService userService, ITelegramBotClient botClient, IOptions<BotConfiguration> botOptions, MemoryCache cache, InsuranceService insuranceService, PdfService pdfService) : base(userService, botClient, botOptions, cache, insuranceService, pdfService)
        {
        }

        protected override Task ProcessDecline(CallbackQuery update)
        {
            return base.ProcessDecline(update);
        }
    }
}
