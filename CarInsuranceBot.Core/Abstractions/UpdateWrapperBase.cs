using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Abstractions
{
    internal abstract class UpdateWrapperBase<TUpdateType>
    {
        protected UpdateWrapperBase(TUpdateType update)
        {
            Update = update;
        }

        public TUpdateType Update;

        public abstract User? GetUser();
    }
}
