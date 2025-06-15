using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Actions.CallbackQueryActions.DocumentsAwait;
using CarInsuranceBot.Core.Actions.CallbackQueryActions.DocumentsConfirmationAwait;
using CarInsuranceBot.Core.Actions.CallbackQueryActions.Home;
using CarInsuranceBot.Core.Actions.CallbackQueryActions.PriceConfirmationAwait;
using CarInsuranceBot.Core.Actions.CallbackQueryActions.PriceSecondConfirmation;
using CarInsuranceBot.Core.Actions.MessageActions.DocumentsAwait;
using CarInsuranceBot.Core.Actions.MessageActions.Home;
using CarInsuranceBot.Core.Actions.MessageActions.None;
using CarInsuranceBot.Core.Actions.MessageActions.Test;
using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using CarInsuranceBot.Core.States.Abstractions;
using CarInsuranceBot.Core.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mindee;
using OpenAI.Chat;
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
                .AddSecretCache(configuration)
                .AddOpenAiServices(configuration)
                .AddApiServices(configuration)
                .AddBotClient()
                .AddServices()
                .AddActions()
                .AddActionFactories()
                .AddHostedService<PollingService>();

            return services;
        }

        private static IServiceCollection AddOpenAiServices(this IServiceCollection services, BotConfiguration configuration)
        {
            if(!string.IsNullOrEmpty(configuration.OpenAiKey))
            {
                services.AddSingleton<ChatClient>(new ChatClient("gpt-4.1-mini", configuration.OpenAiKey));
            }

            return services;
        }

        private static IServiceCollection AddSecretCache(this IServiceCollection services, BotConfiguration configuration)
        {
            services.AddSingleton<IDataCacheBackend, MemoryCacheBackend>(_ => new MemoryCacheBackend());
            services.AddSingleton<DataEncryptionService>(new DataEncryptionService(Convert.FromHexString(configuration.SecretKey)));
            services.AddSingleton<SecretCache>();

            return services;
        }

        private static IServiceCollection AddApiServices(this IServiceCollection services, BotConfiguration configuration)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            services.AddSingleton<MindeeClient>(new MindeeClient(configuration.MindeeKey));

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
                    options.SecretKey = configuration.SecretKey;
                    options.Public256Key = configuration.Public256Key;
                    options.Private256Key = configuration.Private256Key;
                    options.OpenAiKey = configuration.OpenAiKey;
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
            services.AddSingleton<MemoryCache>(new MemoryCache(new MemoryCacheOptions()));

            services.AddScoped<UpdateService>();
            services.AddScoped<ReceiverService>();
            services.AddScoped<UserService>();
            services.AddScoped<DocumentsService>();
            services.AddScoped<InsuranceService>();
            services.AddScoped<PdfService>();
            services.AddScoped<OpenAIService>();
            
            services.AddScoped<TestService>();
            
            return services;
        }

        private static IServiceCollection AddActions(this IServiceCollection services)
        {
            services.AddTransient<HelloMessageAction>();
            services.AddTransient<DefaultHomeMessage>();
            services.AddTransient<TestAction>();

            services.AddTransient<ProcessReconsiderationAction>();
            services.AddTransient<InitCreateInsuranceFlow>();
            services.AddTransient<ProcessDocumentsDataAction>();
            services.AddTransient<ProcessDataConfirmationCallbackAction>();
            services.AddTransient<ProcessPriceConfirmationCallbackAction>();
            services.AddTransient<ProcessSecondPriceConfirmationCallbackAction>();

            return services;
        }

        private static IServiceCollection AddActionFactories(this IServiceCollection services)
        {
            services.AddScoped<ActionsFactory<Message>>(serviceProvider =>
            {
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

                var actions = new Dictionary<UserState, Func<ActionBase<Message>>>
                {
                    { UserState.None, () => scopeFactory.GetAction<Message, HelloMessageAction>() },
                    { UserState.Home, () => scopeFactory.GetAction<Message, DefaultHomeMessage>() },
                    { UserState.DocumentsAwait, () => scopeFactory.GetAction<Message, ProcessDocumentsDataAction>() },
                    { UserState.TestUserState, () => scopeFactory.GetAction<Message, TestAction>() }
                };

                return new ActionsFactory<Message>(actions);
            });

            services.AddScoped<ActionsFactory<CallbackQuery>>(serviceProvider =>
            {
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

                var actions = new Dictionary<UserState, Func<ActionBase<CallbackQuery>>>
                {
                    { UserState.Home, () => scopeFactory.GetAction<CallbackQuery, InitCreateInsuranceFlow>() },
                    { UserState.DocumentsAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessReconsiderationAction>() },
                    { UserState.DocumentsDataConfirmationAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessDataConfirmationCallbackAction>() },
                    { UserState.PriceConfirmationAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessPriceConfirmationCallbackAction>() },
                    { UserState.PriceSecondConfirmationAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessSecondPriceConfirmationCallbackAction >() },
                };

                return new ActionsFactory<CallbackQuery>(actions);
            });

            return services;
        }

        private static ActionBase<TUpdateType> GetAction<TUpdateType, TImplementation>(this IServiceScopeFactory scopeFactory) where TImplementation : ActionBase<TUpdateType>
        {
            return scopeFactory.CreateScope().ServiceProvider.GetRequiredService<TImplementation>();
        }
    }
}
