namespace CarInsuranceBot.Core.Interfaces
{
    internal interface IReceiverService
    {
        Task ReceiveAsync(CancellationToken cancellationToken);
    }
}
