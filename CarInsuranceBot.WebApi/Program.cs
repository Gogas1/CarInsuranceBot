using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.WebApi.Configuration;
using CarInsuranceBot.WebApi.Extensions;

namespace CarInsuranceBot.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ConnectionStringsOptions>(builder.Configuration.GetSection("ConnectionStrings"));

            builder.Services.AddServices();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSqlServerDbContext();

            var botConfig = builder.Configuration.GetSection("TelegramBot").Get<BotConfiguration>();
            ArgumentNullException.ThrowIfNull(botConfig, nameof(botConfig));
            builder.Services.AddCarInsuranceTelegramBot(botConfig);

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
    }
}
