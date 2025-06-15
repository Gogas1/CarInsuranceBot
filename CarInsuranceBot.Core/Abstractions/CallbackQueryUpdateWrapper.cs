using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Abstractions
{
    internal class CallbackQueryUpdateWrapper : UpdateWrapperBase<CallbackQuery>
    {
        public CallbackQueryUpdateWrapper(CallbackQuery update) : base(update)
        {
        }

        public override User? GetUser()
        {
            return Update.From;
        }
    }
}
