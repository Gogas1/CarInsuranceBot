using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Abstractions
{
    /// <summary>
    /// <see cref="UpdateWrapperBase{TUpdateType}"/> implementation for <see cref="CallbackQuery"/> update type
    /// </summary>
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
