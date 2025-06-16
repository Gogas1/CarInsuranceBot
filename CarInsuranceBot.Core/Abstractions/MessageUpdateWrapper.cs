using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Abstractions
{
    /// <summary>
    /// <see cref="UpdateWrapperBase{TUpdateType}"/> implementation for <see cref="Message"/> update type
    /// </summary>
    internal class MessageUpdateWrapper : UpdateWrapperBase<Message>
    {
        public MessageUpdateWrapper(Message update) : base(update)
        {
        }



        public override User? GetUser()
        {
            return Update.From;
        }
    }
}
