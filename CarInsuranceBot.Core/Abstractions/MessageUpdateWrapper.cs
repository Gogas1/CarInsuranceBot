using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Abstractions
{
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
