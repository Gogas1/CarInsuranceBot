using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Cache
{
    internal class SecretCache : ISecretCache
    {
        private readonly IDataCacheBackend _backend;
        private readonly DataEncryptionService _encryptionService;

        public SecretCache(IDataCacheBackend backend, DataEncryptionService encryptionService)
        {
            _backend = backend;
            _encryptionService = encryptionService;
        }

        public Task DeleteAsync(string token)
        {
            return _backend.RemoveAsync(token);
        }

        public async Task<T?> RetrieveAsync<T>(string token)
        {
            var ciphertext = await _backend.GetAsync(token);
            if (ciphertext == null) return default;
            return _encryptionService.Decrypt<T>(ciphertext);
        }

        public async Task<string> StoreAsync<T>(T payload, TimeSpan ttl)
        {
            var token = Guid.NewGuid().ToString("N");
            var ciphertext = _encryptionService.Encrypt(payload);
            await _backend.SetAsync(token, ciphertext, ttl);
            return token;
        }
    }
}
