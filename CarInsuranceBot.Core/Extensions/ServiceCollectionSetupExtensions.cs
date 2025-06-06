using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Actions.MessageActions;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using CarInsuranceBot.Core.States.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Extensions
{
    public static class ServiceCollectionSetupExtensions
    {
        /// <summary>
        /// Registers Car Insurance Bot services
        /// </summary>
        /// <param name="services">IServiceCollection instance</param>
        /// <param name="configuration">Bot configuration</param>
        /// <returns></returns>
        public static IServiceCollection AddCarInsuranceTelegramBot(this IServiceCollection services, BotConfiguration configuration)
        {
            BotConfigValidator.Validate(configuration);

            services.AddBotOptions(configuration)
                .AddBotClient()
                .AddServices()
                .AddActions()
                .AddActionFactories()
                .AddHostedService<PollingService>();

            return services;
        }

        private static IServiceCollection AddBotOptions(this IServiceCollection services, BotConfiguration configuration)
        {
            services
                .AddOptions<BotConfiguration>()
                .Configure(options =>
                {
                    options.Token = configuration.Token;
                    options.AdminIds = configuration.AdminIds;
                });

            return services;
        }

        private static IServiceCollection AddBotClient(this IServiceCollection services)
        {
            services.AddHttpClient("telegram_bot_client")
                .RemoveAllLoggers()
                .AddTypedClient<ITelegramBotClient>((httpClient, serviceProvider) =>
                {
                    BotConfiguration botConfiguration = serviceProvider.GetRequiredService<IOptions<BotConfiguration>>().Value;
                    TelegramBotClientOptions options = new(botConfiguration.Token);
                    var bot = new TelegramBotClient(options, httpClient);
                    Task.WaitAll(bot.DeleteWebhook());
                    return bot;
                });

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<UpdateService>();
            services.AddScoped<ReceiverService>();
            services.AddScoped<UserService>();
            
            return services;
        }

        private static IServiceCollection AddActions(this IServiceCollection services)
        {
            services.AddTransient<HelloMessage>();

            return services;
        }

        private static IServiceCollection AddActionFactories(this IServiceCollection services)
        {
            services.AddSingleton<ActionsFactory<Message>>(serviceProvider =>
            {
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

                var actions = new Dictionary<UserState, Func<ActionBase<Message>>>
                {
                    { UserState.None, () => scopeFactory.CreateScope().ServiceProvider.GetRequiredService<HelloMessage>() },
                };

                return new ActionsFactory<Message>(actions);
            });

            return services;
        }
    }
}
