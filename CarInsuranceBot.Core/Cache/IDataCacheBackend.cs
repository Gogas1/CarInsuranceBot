﻿namespace CarInsuranceBot.Core.Cache
{
    /// <summary>
    /// Data cache backend interface
    /// </summary>
    internal interface IDataCacheBackend
    {
        Task SetAsync(string key, string value, TimeSpan ttl);
        Task<string?> GetAsync(string key);
        Task RemoveAsync(string key);
    }
}
