using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions
{
    /// <summary>
    /// <see cref="ProcessPriceConfirmationCallbackAction"/> implementation action to process user user second agreement or disagreement with price. Handles disagreement differently by cancelling order workflow
    /// <para>Exit states: 
    /// <see cref="Enums.UserState.DocumentsAwait"/> is user's documents data no longer stored, 
    /// <see cref="Enums.UserState.Home"/> is agree, 
    /// <see cref="Enums.UserState.Home"/> is disagree</para>
    /// </summary>
    internal class ProcessSecondPriceConfirmationCallbackAction : ProcessPriceConfirmationCallbackAction
    {
        public ProcessSecondPriceConfirmationCallbackAction(
            UserService userService,
            ITelegramBotClient botClient,
            IOptions<BotConfiguration> botOptions,
            InsuranceService insuranceService,
            PdfService pdfService,
            OpenAIService openAIService,
            DocumentsService documentsService) : base(userService, botClient, botOptions, insuranceService, pdfService, openAIService, documentsService)
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="update"><inheritdoc/></param>
        /// <param name="cancellationToken"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override async Task ProcessDecline(CallbackQuery update, CancellationToken cancellationToken)
        {
            // Send message about second disagreement and returning to the home state. Sending home actions keyboard
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.SECOND_PRICE_DECLINE_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.Home, update.From.Id, cancellationToken);
        }
    }
}
