using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Configuration
{
    public class BotConfiguration
    {
        public required string Token { get; set; } = string.Empty;
        public List<long> AdminIds { get; set; } = new();

        /// <summary>
        /// 32 bytes long key
        /// </summary>
        public required string SecretKey { get; set; } = string.Empty;
        public required string Public256Key { get; set; } = string.Empty;
        public required string Private256Key { get; set; } = string.Empty;
        public required string MindeeKey { get; set; } = string.Empty;

        public BotConfiguration()
        {
            
        }

        public BotConfiguration(string token, string secretKey, string public256Key, string private256Key, string mindeeKey)
        {
            Token = token;
            SecretKey = secretKey;
            Public256Key = public256Key;
            Private256Key = private256Key;
            MindeeKey = mindeeKey;
        }

        public BotConfiguration(string token, string secretKey, List<long> adminIds, string public256Key, string private256Key, string mindeeKey) 
            : this(token, secretKey, public256Key, private256Key, mindeeKey)
        {
            AdminIds = adminIds;
        }
    }
}
