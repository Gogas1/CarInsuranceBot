using CarInsuranceBot.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarInsuranceBot.Core.Services
{
    internal class PollingService : PollingServiceBase<ReceiverService>
    {
        public PollingService(IServiceProvider serviceProvider) : base(serviceProvider, serviceProvider.GetRequiredService<ILogger<PollingServiceBase<ReceiverService>>>())
        {
        }
    }
}
