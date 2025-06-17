using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions.PriceConfirmationShared
{
    internal class PriceConfirmationMessageAction : MessageActionBase
    {
        public PriceConfirmationMessageAction(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        protected override async Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                update.Chat,
                AnswersData.MUST_USE_KEYBOARD_TEXT,
                replyMarkup: AnswersData.PRICE_CONFIRMATION_KEYBOARD,
                cancellationToken: cancellationToken);
        }
    }
}
