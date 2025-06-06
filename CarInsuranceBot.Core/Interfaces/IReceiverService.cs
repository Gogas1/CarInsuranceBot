using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Interfaces
{
    internal interface IReceiverService
    {
        Task ReceiveAsync(CancellationToken cancellationToken);
    }
}
