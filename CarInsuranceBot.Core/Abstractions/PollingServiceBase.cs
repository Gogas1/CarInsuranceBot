using CarInsuranceBot.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CarInsuranceBot.Core.Abstractions
{
    /// <summary>
    /// <see cref="BackgroundService"/> implementation for polling generic service
    /// </summary>
    /// <typeparam name="TReceiverSerivce">Concrete receiver type that implements <see cref="IReceiverService"/></typeparam>
    internal class PollingServiceBase<TReceiverSerivce> : BackgroundService where TReceiverSerivce : IReceiverService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PollingServiceBase<TReceiverSerivce>> _logger;

        public PollingServiceBase(IServiceProvider serviceProvider, ILogger<PollingServiceBase<TReceiverSerivce>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartReceiving(stoppingToken);
        }

        /// <summary>
        /// Starts receiving using <see cref="TReceiverSerivce"/>
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task StartReceiving(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var receiver = scope.ServiceProvider.GetRequiredService<TReceiverSerivce>();
                    _logger.LogInformation("Receiving");
                    await receiver.ReceiveAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}
