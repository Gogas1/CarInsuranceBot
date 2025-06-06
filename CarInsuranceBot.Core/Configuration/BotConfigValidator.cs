using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Configuration
{
    /// <summary>
    /// Provides methods to validate <see cref="BotConfiguration"/> data
    /// </summary>
    internal static class BotConfigValidator
    {
        /// <summary>
        /// <para>Validates <see cref="BotConfiguration"/> class instance data and throws <see cref="ArgumentNullException"/> if invalid</para>
        /// <para>Validated properties:</para>
        /// <para>1. <see cref="BotConfiguration.Token"/> - not null or empty</para>
        /// </summary>
        /// <param name="configuration">Class instance to validate</param>
        /// <exception cref="ArgumentNullException"></exception>
        internal static void Validate(BotConfiguration configuration)
        {
            if(configuration == null) throw new ArgumentNullException(nameof(configuration), "Bot configuration object cannot be null");

            if (string.IsNullOrEmpty(configuration.Token)) throw new ArgumentNullException(nameof(configuration.Token), "Bot token cannot be empty");
        }
    }
}
