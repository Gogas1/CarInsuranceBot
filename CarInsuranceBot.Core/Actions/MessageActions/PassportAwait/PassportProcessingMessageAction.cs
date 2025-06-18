using CarInsuranceBot.Core.Actions.MessageActions.Abstractions;
using CarInsuranceBot.Core.Services;
using Mindee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions.PassportAwait
{
    internal class PassportProcessingMessageAction : ChatDocumentProcessingAction
    {
        public PassportProcessingMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            MindeeClient mindeeClient) : base(userService, botClient, mindeeClient)
        {
        }

        protected override Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            if(!update.Photo?.Any() ?? true)
            {
                // TODO Tell there is no photo
            }
        }
    }
}
