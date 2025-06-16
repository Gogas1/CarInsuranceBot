using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Abstractions
{
    /// <summary>
    /// Telegram update wrapper base to provide access to a common data
    /// </summary>
    /// <typeparam name="TUpdateType">Concrete update type</typeparam>
    internal abstract class UpdateWrapperBase<TUpdateType>
    {

        /// <param name="update">Update instance</param>
        protected UpdateWrapperBase(TUpdateType update)
        {
            Update = update;
        }

        /// <summary>
        /// Wrapped update instance
        /// </summary>
        public TUpdateType Update;

        /// <returns><see cref="User"/> instance from the update instance</returns>
        public abstract User? GetUser();
    }
}
