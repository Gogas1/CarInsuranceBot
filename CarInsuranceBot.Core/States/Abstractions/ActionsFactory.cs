using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Enums;

namespace CarInsuranceBot.Core.States.Abstractions
{
    /// <summary>
    /// Actions factory
    /// </summary>
    /// <typeparam name="TUpdateType"></typeparam>
    internal class ActionsFactory<TUpdateType>
    {
        private readonly Dictionary<UserState, Func<ActionBase<TUpdateType>>> _actions;

        public ActionsFactory(Dictionary<UserState, Func<ActionBase<TUpdateType>>> actions)
        {
            _actions = actions;
        }

        /// <summary>
        /// Resolves action for the user state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionBase<TUpdateType>? GetActionForState(UserState state)
        {
            if (_actions.TryGetValue(state, out var action))
            {
                return action();
            }

            return null;
        }
    }
}
