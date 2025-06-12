using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
