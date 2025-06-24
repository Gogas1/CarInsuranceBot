using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions
{
    internal class ProcessPassportDataCorrectionCallbackQueryAction : ProcessDataCorrectionCallbackQueryAction
    {
        public ProcessPassportDataCorrectionCallbackQueryAction(UserService userService, ITelegramBotClient botClient, OpenAIService openAiService) : base(userService, botClient, openAiService)
        {
            
        }

        protected override UserState ExitState => UserState.PassportAwait;

        protected override async Task<string> GetMessageAsync(CancellationToken cancellationToken)
        {
            return await _openAiService.GetDiversifiedAnswer(AnswersData.SHARE_PASSPORT_IN_CHAT_GPT_SETTINGS, cancellationToken);
        }
    }
}
