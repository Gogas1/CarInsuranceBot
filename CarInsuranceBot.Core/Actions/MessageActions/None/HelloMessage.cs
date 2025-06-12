using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using CarInsuranceBot.Core.States.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Actions.MessageActions.None
{
    internal class HelloMessage : MessageActionBase
    {

        private string _helloText = "Hello. I am a car insurance bot. I can assist you with car insurance purchases";

        public HelloMessage(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        public override async Task Execute(Message update)
        {
            if (update.From == null)
            {
                return;
            }

            var inlineKeyboard = new InlineKeyboardButton("Get Insurance", "get_insurance");
            await _botClient.SendMessage(update.Chat, _helloText, replyMarkup: inlineKeyboard);
            await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id);
        }
    }
}
