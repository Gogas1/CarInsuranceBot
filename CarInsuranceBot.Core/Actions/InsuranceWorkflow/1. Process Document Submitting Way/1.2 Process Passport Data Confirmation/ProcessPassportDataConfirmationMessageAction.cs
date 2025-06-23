using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions.PassportDataConfirmationAwait
{
    internal class ProcessPassportDataConfirmationMessageAction : MessageActionBase
    {
        public ProcessPassportDataConfirmationMessageAction(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        protected override Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
