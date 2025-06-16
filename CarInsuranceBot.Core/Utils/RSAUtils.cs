using System.Security.Cryptography;

namespace CarInsuranceBot.Core.Utils
{
    /// <summary>
    /// Utils to work with RSA
    /// </summary>
    internal static class RSAUtils
    {
        /// <summary>
        /// Creates RSA instance from the key string
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static RSA GetRSAFromString(string src)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(src);
            return rsa;
        }
    }
}
