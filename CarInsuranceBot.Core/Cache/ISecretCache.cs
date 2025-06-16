namespace CarInsuranceBot.Core.Cache
{
    /// <summary>
    /// Encrypted cache service interface
    /// </summary>
    internal interface ISecretCache
    {
        Task<string> StoreAsync<T>(T payload, TimeSpan ttl);
        Task<T?> RetrieveAsync<T>(string token);
        Task DeleteAsync(string token);
    }
}
