namespace CarInsuranceBot.Core.Interfaces
{
    /// <summary>
    /// Receiver service interface
    /// </summary>
    internal interface IReceiverService
    {
        Task ReceiveAsync(CancellationToken cancellationToken);
    }
}
