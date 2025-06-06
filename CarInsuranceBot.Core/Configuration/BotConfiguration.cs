using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Configuration
{
    public class BotConfiguration
    {
        public required string Token { get; set; } = string.Empty;
        public List<long> AdminIds { get; set; } = new();

        public BotConfiguration()
        {
            
        }

        public BotConfiguration(string token)
        {
            Token = token;
        }

        public BotConfiguration(string token, List<long> adminIds) : this(token)
        {
            AdminIds = adminIds;
        }
    }
}
