﻿using CarInsuranceBot.Core.Configuration;

namespace CarInsuranceBot.Core.Validation
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
            if (configuration == null) throw new ArgumentNullException(nameof(configuration), "Bot configuration object cannot be null");

            if (string.IsNullOrEmpty(configuration.Token)) throw new ArgumentNullException(nameof(configuration.Token), "Bot token cannot be empty");
            if (string.IsNullOrEmpty(configuration.Public256Key)) throw new ArgumentNullException(nameof(configuration.Public256Key), "Public rsa key needed for documents processing");
            if (string.IsNullOrEmpty(configuration.Private256Key)) throw new ArgumentNullException(nameof(configuration.Private256Key), "Private rsa key needed for documents processing");
            if (string.IsNullOrEmpty(configuration.MindeeKey)) throw new ArgumentNullException(nameof(configuration.MindeeKey), "Mindee api key needed for documents processing");
        }
    }
}
