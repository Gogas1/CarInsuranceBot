using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.WebApi.Configuration;
using CarInsuranceBot.WebApi.Extensions;

namespace CarInsuranceBot.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure connection strings
            builder.Services.Configure<ConnectionStringsOptions>(builder.Configuration.GetSection("ConnectionStrings"));

            // Register services
            builder.Services.AddServices();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Register db context for the provider
            builder.Services.AddSqlServerDbContext(true);

            // Setup telegram bot services
            await SetupTelegramBot(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static async Task SetupTelegramBot(WebApplicationBuilder builder)
        {
            // Get bot config
            var botConfig = builder.Configuration.GetSection("TelegramBot").Get<BotConfiguration>();
            // Get keys config
            var keysCofig = builder.Configuration.GetSection("Keys").Get<KeysOptions>();

            ArgumentNullException.ThrowIfNull(botConfig, nameof(botConfig));
            // If keys provided by a bot config
            if(!string.IsNullOrEmpty(botConfig.Public256Key) && !string.IsNullOrEmpty(botConfig.Private256Key))
            {
                // Add telegram bot services
                builder.Services.AddCarInsuranceTelegramBot(botConfig);
                return;
            }

            //If no keys at all
            if (keysCofig == null || string.IsNullOrEmpty(keysCofig.Public256KeyPath) || string.IsNullOrEmpty(keysCofig.Private256KeyPath))
            {
                throw new ArgumentNullException("Setup Keys.Public256KeyPath and Keys.Private256KeyPath configuration");
            }

            // Read keys from files
            botConfig.Public256Key = await File.ReadAllTextAsync(keysCofig.Public256KeyPath);
            botConfig.Private256Key = await File.ReadAllTextAsync(keysCofig.Private256KeyPath);

            // Add telegram bot services
            builder.Services.AddCarInsuranceTelegramBot(botConfig);
        }
    }
}
