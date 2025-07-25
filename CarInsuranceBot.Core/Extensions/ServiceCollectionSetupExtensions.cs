﻿using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Actions.CallbackQueryActions;
using CarInsuranceBot.Core.Actions.DefaultActions;
using CarInsuranceBot.Core.Actions.MessageActions;
using CarInsuranceBot.Core.Actions.MessageActions.PassportDataConfirmationAwait;
using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using CarInsuranceBot.Core.States.Abstractions;
using CarInsuranceBot.Core.Validation;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mindee;
using OpenAI;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extenstions to setup Car Insurance Telegram Bot and its services
    /// </summary>
    public static class ServiceCollectionSetupExtensions
    {
        /// <summary>
        /// Registers Car Insurance Bot services, actions and polling service
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance</param>
        /// <param name="configuration">Bot configuration</param>
        /// <returns>Passed <see cref="IServiceCollection"/> instance</returns>
        public static IServiceCollection AddCarInsuranceTelegramBot(this IServiceCollection services, BotConfiguration configuration)
        {
            BotConfigValidator.Validate(configuration);

            // Register all needed services
            services.AddBotOptions(configuration)
                .AddSecretCache(configuration)
                .AddOpenAiServices(configuration, "gpt-4.1-mini")
                .AddApiServices(configuration)
                .AddBotClient()
                .AddServices()
                .AddActions()
                .AddActionFactories()
                .AddHostedService<PollingService>();

            return services;
        }

        /// <summary>
        /// Registers OpenAI ChatClient
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance</param>
        /// <param name="configuration">Bot configuration</param>
        /// <param name="model">Gpt model to use</param>
        /// <returns>Passed <see cref="IServiceCollection"/> instance</returns>
        private static IServiceCollection AddOpenAiServices(this IServiceCollection services, BotConfiguration configuration, string model)
        {
            services.AddSingleton<IChatClient>(new ChatClientBuilder(new OpenAIClient(configuration.OpenAiKey).GetChatClient(model).AsIChatClient())
                .UseFunctionInvocation()
                .Build());

            return services;
        }

        /// <summary>
        /// Registers encrypted cache services
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance</param>
        /// <param name="configuration">Bot configuration</param>
        /// <returns>Passed <see cref="IServiceCollection"/> instance</returns>
        private static IServiceCollection AddSecretCache(this IServiceCollection services, BotConfiguration configuration)
        {
            services.AddSingleton<IDataCacheBackend, MemoryCacheBackend>(_ => new MemoryCacheBackend());
            services.AddSingleton<DataEncryptionService>(new DataEncryptionService(Convert.FromHexString(configuration.SecretKey)));
            services.AddSingleton<SecretCache>();

            return services;
        }

        /// <summary>
        /// Registers api services and configuration: Mindee, QuestPDF
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance</param>
        /// <param name="configuration">Bot configuration</param>
        /// <returns>Passed <see cref="IServiceCollection"/> instance</returns>
        private static IServiceCollection AddApiServices(this IServiceCollection services, BotConfiguration configuration)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            services.AddSingleton<MindeeClient>(new MindeeClient(configuration.MindeeKey));

            return services;
        }

        /// <summary>
        /// Registers <see cref="BotConfiguration"/>
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance</param>
        /// <param name="configuration">Bot configuration</param>
        /// <returns>Passed <see cref="IServiceCollection"/> instance</returns>
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

        /// <summary>
        /// Registers and configures bot client
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance</param>
        /// <returns>Passed <see cref="IServiceCollection"/> instance</returns>
        private static IServiceCollection AddBotClient(this IServiceCollection services)
        {
            //Registers telegram bot client with http client for it
            services.AddHttpClient("telegram_bot_client")
                .RemoveAllLoggers()
                .AddTypedClient<ITelegramBotClient>((httpClient, serviceProvider) =>
                {
                    BotConfiguration botConfiguration = serviceProvider.GetRequiredService<IOptions<BotConfiguration>>().Value;
                    TelegramBotClientOptions options = new(botConfiguration.Token);
                    var bot = new TelegramBotClient(options, httpClient);
                    bot.DeleteWebhook().Wait();
                    return bot;
                });

            return services;
        }

        /// <summary>
        /// Registers telegram bot core services
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance</param>
        /// <returns>Passed <see cref="IServiceCollection"/> instance</returns>
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
            //services.AddScoped<TestService>();

            return services;
        }

        /// <summary>
        /// Registers bot actions
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance</param>
        /// <returns>Passed <see cref="IServiceCollection"/> instance</returns>
        private static IServiceCollection AddActions(this IServiceCollection services)
        {
            //Home
            services.AddTransient<DefaultMessageAction>();
            services.AddTransient<HandleHomeMessageAction>();

            services.AddTransient<ProcessDocumentsSubmittingMessageAction>();
            services.AddTransient<PassportProcessingMessageAction>();
            services.AddTransient<ProcessPassportDataCorrectionMessageAction>();
            services.AddTransient<ProcessPassportDataConfirmationMessageAction>();
            services.AddTransient<LicenseProcessingMessageAction>();
            services.AddTransient<ProcessVehicleIdDataConfirmationMessageAction>();
            services.AddTransient<ProcessVehicleIdDataCorrectionMessageAction>();
            services.AddTransient<ProcessDataConfirmationMessageAction>();
            services.AddTransient<PriceConfirmationMessageAction>();

            services.AddTransient<DefaultCallbackQueryAction>();
            services.AddTransient<HandleHomeCallbackQueryAction>();

            services.AddTransient<PassportProcessingCallbackQueryAction>();
            services.AddTransient<ProcessPassportDataConfirmationCallbackAction>();
            services.AddTransient<ProcessPassportDataCorrectionCallbackQueryAction>();
            services.AddTransient<ProcessVehicleIdDataConfirmationCallbackAction>();
            services.AddTransient<ProcessVehicleIdDataCorrectionCallbackQueryAction>();
            services.AddTransient<ProcessDocumentsSubmittingCallbackQueryAction>();
            services.AddTransient<ProcessDataConfirmationCallbackAction>();
            services.AddTransient<ProcessPriceConfirmationCallbackAction>();
            services.AddTransient<ProcessSecondPriceConfirmationCallbackAction>();
            services.AddTransient<ProcessPassportDataConfirmationCallbackAction>();
            services.AddTransient<ProcessVehicleIdDataCorrectionCallbackQueryAction>();

            return services;
        }

        /// <summary>
        /// Registers bot action factories
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance</param>
        /// <returns>Passed <see cref="IServiceCollection"/> instance</returns>
        private static IServiceCollection AddActionFactories(this IServiceCollection services)
        {
            services.AddScoped<ActionsFactory<Message>>(serviceProvider =>
            {
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

                var actions = new Dictionary<UserState, Func<ActionBase<Message>>>
                {
                    { UserState.Home, () => scopeFactory.GetAction<Message, HandleHomeMessageAction>() },
                    { UserState.DocumentsAwait, () => scopeFactory.GetAction<Message, ProcessDocumentsSubmittingMessageAction>() },
                    { UserState.DocumentsDataConfirmationAwait, () => scopeFactory.GetAction<Message, ProcessDataConfirmationMessageAction>() },
                    { UserState.PriceConfirmationAwait, () => scopeFactory.GetAction<Message, PriceConfirmationMessageAction>() },
                    { UserState.PriceSecondConfirmationAwait, () => scopeFactory.GetAction<Message, PriceConfirmationMessageAction>() },
                    { UserState.PassportAwait, () => scopeFactory.GetAction<Message, PassportProcessingMessageAction>() },
                    { UserState.PassportDataConfirmationAwait, () => scopeFactory.GetAction<Message, ProcessPassportDataConfirmationMessageAction>() },
                    { UserState.LicenseAwait, () => scopeFactory.GetAction<Message, LicenseProcessingMessageAction>() },
                    { UserState.LicenseDataConfirmationAwait, () => scopeFactory.GetAction<Message, ProcessVehicleIdDataConfirmationMessageAction>() },
                    { UserState.LicenseDataCorrectionAwait, () => scopeFactory.GetAction<Message, ProcessVehicleIdDataCorrectionMessageAction>() },
                    { UserState.PassportDataCorrectionAwait, () => scopeFactory.GetAction<Message, ProcessPassportDataCorrectionMessageAction>() }
                };

                return new ActionsFactory<Message>(actions, () => scopeFactory.GetAction<Message, DefaultMessageAction>());
            });

            services.AddScoped<ActionsFactory<CallbackQuery>>(serviceProvider =>
            {
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

                var actions = new Dictionary<UserState, Func<ActionBase<CallbackQuery>>>
                {
                    { UserState.Home, () => scopeFactory.GetAction<CallbackQuery, HandleHomeCallbackQueryAction>() },
                    { UserState.DocumentsAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessDocumentsSubmittingCallbackQueryAction>() },
                    { UserState.DocumentsDataConfirmationAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessDataConfirmationCallbackAction>() },
                    { UserState.PriceConfirmationAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessPriceConfirmationCallbackAction>() },
                    { UserState.PriceSecondConfirmationAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessSecondPriceConfirmationCallbackAction>() },
                    { UserState.PassportDataConfirmationAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessPassportDataConfirmationCallbackAction>() },
                    { UserState.LicenseDataConfirmationAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessVehicleIdDataConfirmationCallbackAction>() },
                    { UserState.PassportDataCorrectionAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessPassportDataCorrectionCallbackQueryAction>() },
                    { UserState.LicenseDataCorrectionAwait, () => scopeFactory.GetAction<CallbackQuery, ProcessVehicleIdDataCorrectionCallbackQueryAction>() }
                };

                return new ActionsFactory<CallbackQuery>(actions, () => scopeFactory.GetAction<CallbackQuery, DefaultCallbackQueryAction>());
            });

            return services;
        }

        /// <summary>
        /// Helper method to abstract retrieving <see cref="ActionBase{TUpdateType}"/> from <see cref="IServiceScopeFactory"/>
        /// </summary>
        /// <typeparam name="TUpdateType">Target action update type</typeparam>
        /// <typeparam name="TImplementation">Target <see cref="ActionBase{TUpdateType}"/> implementation type</typeparam>
        /// <param name="scopeFactory"><see cref="IServiceScopeFactory"/> instance</param>
        /// <returns>Target <see cref="ActionBase{TUpdateType}"/> implementation</returns>
        private static ActionBase<TUpdateType> GetAction<TUpdateType, TImplementation>(this IServiceScopeFactory scopeFactory) where TImplementation : ActionBase<TUpdateType>
        {
            return scopeFactory.CreateScope().ServiceProvider.GetRequiredService<TImplementation>();
        }
    }
}
