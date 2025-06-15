using CarInsuranceBot.Core.Actions.CallbackQueryActions.PriceConfirmationAwait;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.PriceSecondConfirmation
{
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

        protected override async Task ProcessDecline(CallbackQuery update, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                update.From.Id,
                await _openAiService.GetDiversifiedAnswer(AnswersData.SECOND_PRICE_DECLINE_SETTINGS, cancellationToken),
                replyMarkup: AnswersData.HOME_KEYBOARD,
                cancellationToken: cancellationToken);
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.Home, update.From.Id, cancellationToken);
        }
    }
}
