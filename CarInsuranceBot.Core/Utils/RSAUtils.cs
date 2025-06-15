using System.Security.Cryptography;

namespace CarInsuranceBot.Core.Utils
{
    internal static class RSAUtils
    {
        public static RSA GetRSAFromString(string src)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(src);
            return rsa;
        }
    }
}
