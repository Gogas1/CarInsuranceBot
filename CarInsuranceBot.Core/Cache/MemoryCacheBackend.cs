using Microsoft.Extensions.Caching.Memory;

namespace CarInsuranceBot.Core.Cache
{
    /// <summary>
    /// <see cref="MemoryCache"/> wrapper
    /// </summary>
    internal class MemoryCacheBackend : IDataCacheBackend, IDisposable
    {
        private readonly MemoryCache _cache;

        public MemoryCacheBackend()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public void Dispose()
        {
            _cache.Dispose();
        }

        public Task<string?> GetAsync(string key)
        {
            _cache.TryGetValue(key, out string? value);
            return Task.FromResult(value);
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task SetAsync(string key, string value, TimeSpan ttl)
        {
            _cache.Set(key, value, ttl);
            return Task.CompletedTask;
        }
    }
}
