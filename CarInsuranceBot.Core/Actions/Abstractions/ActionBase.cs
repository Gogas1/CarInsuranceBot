using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.Abstractions
{
    internal abstract class ActionBase<TUpdateType>
    {
        public abstract Task Execute(TUpdateType update);
    }
}
