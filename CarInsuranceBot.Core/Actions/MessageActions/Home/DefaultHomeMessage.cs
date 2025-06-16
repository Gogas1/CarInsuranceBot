using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.MessageActions.Home
{
    /// <summary>
    /// <see cref="MessageActionBase"/> implementation to handle home state of the bot.
    /// </summary>
    internal class DefaultHomeMessage : MessageActionBase
    {
        private readonly OpenAIService _openAiService;

        public DefaultHomeMessage(UserService userService, ITelegramBotClient botClient, OpenAIService openAIService) : base(userService, botClient)
        {
            _openAiService = openAIService;
        }

        protected override async Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            if (update.From == null)
            {
                return;
            }

            // Send home message and available actions keyboard
            await _botClient.SendMessage(
                update.Chat,
                await _openAiService.GetDiversifiedAnswer(AnswersData.HOME_MESSAGE_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);

            // Change user state
            await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id, cancellationToken);
        }
    }
}
