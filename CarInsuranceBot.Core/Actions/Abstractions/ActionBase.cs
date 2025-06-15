using CarInsuranceBot.Core.Abstractions;

namespace CarInsuranceBot.Core.Actions.Abstractions
{
    internal abstract class ActionBase<TUpdateType>
    {
        public abstract Task Execute(UpdateWrapperBase<TUpdateType> update, CancellationToken cancellationToken);
    }
}
