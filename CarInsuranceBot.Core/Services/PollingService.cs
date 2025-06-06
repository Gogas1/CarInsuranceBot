using CarInsuranceBot.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Services
{
    internal class PollingService : PollingServiceBase<ReceiverService>
    {
        public PollingService(IServiceProvider serviceProvider) : base(serviceProvider, serviceProvider.GetRequiredService<ILogger<PollingServiceBase<ReceiverService>>>())
        {
        }
    }
}
