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

namespace CarInsuranceBot.Core.Actions.MessageActions
{
    internal class HelloMessage : ActionBase<Message>
    {
        protected readonly ITelegramBotClient _botClient;

        private string _helloText = "Hello. I am a car insurance bot. I can assist you with car insurance purchases";

        public HelloMessage(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        //public HelloMessage(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        //{
        //}

        public override async Task Execute(Message update)
        {
            await _botClient.SendMessage(update.Chat, "HELLLLO");
        }
    }
}
