using CarInsuranceBot.Core.Abstractions;

namespace CarInsuranceBot.Core.Actions.Abstractions
{
    /// <summary>
    /// Abstract generic action base.
    /// </summary>
    /// <typeparam name="TUpdateType">Concrete telegram bot update type</typeparam>
    internal abstract class ActionBase<TUpdateType>
    {
        /// <summary>
        /// Action main execution method
        /// </summary>
        /// <param name="update">Update wrapper</param>
        /// <param name="cancellationToken">External cancellation token</param>
        /// <returns><see cref="Task"/></returns>
        public abstract Task Execute(UpdateWrapperBase<TUpdateType> update, CancellationToken cancellationToken);
    }
}
