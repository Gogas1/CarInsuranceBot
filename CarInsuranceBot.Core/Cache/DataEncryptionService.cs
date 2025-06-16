using System.Security.Cryptography;
using System.Text.Json;

namespace CarInsuranceBot.Core.Cache
{
    /// <summary>
    /// Service to encrypt data
    /// </summary>
    internal class DataEncryptionService
    {
        private readonly byte[] _key;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">32 bytes long key</param>
        /// <exception cref="ArgumentException"></exception>
        public DataEncryptionService(byte[] key)
        {
            if (key == null || key.Length != 32)
            {
                throw new ArgumentException("Key must be 32 bytes long", nameof(key));
            }

            _key = key;
        }

        /// <summary>
        /// Encrypts provided object
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="payload">Payload</param>
        /// <returns>Encrypted json representation as Base64 string</returns>
        public string Encrypt<T>(T payload)
        {
            // Serialize payload
            var plaintext = JsonSerializer.SerializeToUtf8Bytes(payload);

            var nonce = RandomNumberGenerator.GetBytes(12);
            var cipher = new byte[plaintext.Length];
            var tag = new byte[16];

            using var aesGcm = new AesGcm(_key, tagSizeInBytes: 16);
            aesGcm.Encrypt(nonce, plaintext, cipher, tag);

            // Combine all the data into byte array
            var combined = new byte[nonce.Length + tag.Length + cipher.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
            Buffer.BlockCopy(cipher, 0, combined, nonce.Length + tag.Length, cipher.Length);

            return Convert.ToBase64String(combined);
        }

        public T? Decrypt<T>(string ciphertextB64)
        {
            try
            {
                var combined = Convert.FromBase64String(ciphertextB64);
                var nonce = combined.AsSpan(0, 12).ToArray();
                var tag = combined.AsSpan(12, 16).ToArray();
                var cipher = combined.AsSpan(28).ToArray();
                var plaintext = new byte[cipher.Length];

                using var aesGcm = new AesGcm(_key, tagSizeInBytes: 16);
                aesGcm.Decrypt(nonce, cipher, tag, plaintext);

                return JsonSerializer.Deserialize<T>(plaintext);
            }
            catch
            {
                return default;
            }
        }
    }
}
