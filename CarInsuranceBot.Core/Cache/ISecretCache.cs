using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Cache
{
    internal interface ISecretCache
    {
        Task<string> StoreAsync<T>(T payload, TimeSpan ttl);
        Task<T?> RetrieveAsync<T>(string token);
        Task DeleteAsync(string token);
    }
}
