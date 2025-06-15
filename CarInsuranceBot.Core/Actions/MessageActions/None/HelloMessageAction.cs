using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Actions.MessageActions.None
{
    internal class HelloMessageAction : MessageActionBase
    {

        private readonly OpenAIService _openAiService;

        public HelloMessageAction(UserService userService, ITelegramBotClient botClient, OpenAIService openAiService) : base(userService, botClient)
        {
            _openAiService = openAiService;
        }

        protected override async Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            var inlineKeyboard = new InlineKeyboardButton("Get Insurance", "get_insurance");
            await _botClient.SendMessage(
                update.Chat,
                await _openAiService.GetDiversifiedAnswer(AnswersData.HELLO_MESSAGE_SETTINGS, cancellationToken),
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
            await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id, cancellationToken);
        }
    }
}
