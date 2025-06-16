using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Actions.MessageActions.None
{
    /// <summary>
    /// <see cref="MessageActionBase"/> implementation to process new users
    /// </summary>
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

            // Send hello message and available actions keyboard
            await _botClient.SendMessage(
                update.Chat,
                await _openAiService.GetDiversifiedAnswer(AnswersData.HELLO_MESSAGE_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id, cancellationToken);
        }
    }
}
