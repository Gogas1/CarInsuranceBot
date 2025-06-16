namespace CarInsuranceBot.Core.Cache
{

    /// <summary>
    /// Encrypted cache service
    /// </summary>
    internal class SecretCache : ISecretCache
    {
        private readonly IDataCacheBackend _backend;
        private readonly DataEncryptionService _encryptionService;

        public SecretCache(IDataCacheBackend backend, DataEncryptionService encryptionService)
        {
            _backend = backend;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Deletes target cached item
        /// </summary>
        /// <param name="token">Cached item token</param>
        /// <returns></returns>
        public Task DeleteAsync(string token)
        {
            return _backend.RemoveAsync(token);
        }

        /// <summary>
        /// Retrieves target object
        /// </summary>
        /// <typeparam name="T">Target object type</typeparam>
        /// <param name="token">Target object key</param>
        /// <returns>Target object instance if present otherwise null</returns>
        public async Task<T?> RetrieveAsync<T>(string token)
        {
            // Get stored encrypted object string
            var ciphertext = await _backend.GetAsync(token);
            if (ciphertext == null) return default;
            // Decrypt it if cached item is present
            return _encryptionService.Decrypt<T>(ciphertext);
        }

        /// <summary>
        /// Stores target object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="payload">Object payload</param>
        /// <param name="ttl">Cached item life time</param>
        /// <returns>Object cache key</returns>
        public async Task<string> StoreAsync<T>(T payload, TimeSpan ttl)
        {
            // Create key
            var token = Guid.NewGuid().ToString("N");
            // Encrypt object
            var ciphertext = _encryptionService.Encrypt(payload);
            // Store object
            await _backend.SetAsync(token, ciphertext, ttl);
            return token;
        }
    }
}
