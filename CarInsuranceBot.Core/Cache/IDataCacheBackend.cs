using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Cache
{
    internal interface IDataCacheBackend
    {
        Task SetAsync(string key, string value, TimeSpan ttl);
        Task<string?> GetAsync(string key);
        Task RemoveAsync(string key);
    }
}
