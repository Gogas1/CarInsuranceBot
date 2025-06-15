using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.WebApi.Configuration;
using CarInsuranceBot.WebApi.Extensions;
using System.Threading.Tasks;

namespace CarInsuranceBot.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ConnectionStringsOptions>(builder.Configuration.GetSection("ConnectionStrings"));

            builder.Services.AddServices();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSqlServerDbContext();

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
            var botConfig = builder.Configuration.GetSection("TelegramBot").Get<BotConfiguration>();
            var keysCofig = builder.Configuration.GetSection("Keys").Get<KeysOptions>();

            ArgumentNullException.ThrowIfNull(botConfig, nameof(botConfig));
            if(keysCofig == null || string.IsNullOrEmpty(keysCofig.Public256KeyPath) || string.IsNullOrEmpty(keysCofig.Private256KeyPath))
            {
                throw new ArgumentNullException("Setup Keys.Public256KeyPath and Keys.Private256KeyPath configuration");
            }

            botConfig.Public256Key = await File.ReadAllTextAsync(keysCofig.Public256KeyPath);
            botConfig.Private256Key = await File.ReadAllTextAsync(keysCofig.Private256KeyPath);

            builder.Services.AddCarInsuranceTelegramBot(botConfig);
        }
    }
}
