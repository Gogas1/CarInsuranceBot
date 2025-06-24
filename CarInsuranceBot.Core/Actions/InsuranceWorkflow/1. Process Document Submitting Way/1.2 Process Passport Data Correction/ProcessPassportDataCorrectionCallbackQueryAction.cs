using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.InsuranceWorkflow._1._Process_Document_Submitting_Way._1._2_Process_Passport_Data_Correction
{
    internal class ProcessPassportDataCorrectionCallbackQueryAction : CallbackQueryActionBase
    {


        public ProcessPassportDataCorrectionCallbackQueryAction(UserService userService, ITelegramBotClient botClient) : base(userService, botClient)
        {
        }

        protected override Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
