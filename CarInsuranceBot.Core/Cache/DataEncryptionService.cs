using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Cache
{
    internal class DataEncryptionService
    {
        private readonly byte[] _key;

        public DataEncryptionService(byte[] key)
        {
            if(key == null || key.Length != 32)
            {
                throw new ArgumentException("Key must be 32 bytes long", nameof(key));
            }

            _key = key;
        }

        public string Encrypt<T>(T payload)
        {
            var plaintext = JsonSerializer.SerializeToUtf8Bytes(payload);

            var nonce = RandomNumberGenerator.GetBytes(12);
            var cipher = new byte[plaintext.Length];
            var tag = new byte[16];

            using var aesGcm = new AesGcm(_key, tagSizeInBytes: 16);
            aesGcm.Encrypt(nonce, plaintext, cipher, tag);

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
