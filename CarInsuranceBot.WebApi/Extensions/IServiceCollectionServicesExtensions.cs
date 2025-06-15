using CarInsuranceBot.Core.Interfaces.Repositories;
using CarInsuranceBot.WebApi.Configuration;
using CarInsuranceBot.WebApi.DbContexts;
using CarInsuranceBot.WebApi.Interfaces;
using CarInsuranceBot.WebApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CarInsuranceBot.WebApi.Extensions
{
    public static class IServiceCollectionServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ITelegramBotUserRepository, UserRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }

        public static IServiceCollection AddSqlServerDbContext(this IServiceCollection services, bool useAzureSql)
        {
            services.AddDbContext<CarInsuranceDbContext>((serviceProvider, options) =>
            {
                ConnectionStringsOptions? connectionStringsOptions = serviceProvider.GetService<IOptions<ConnectionStringsOptions>>()?.Value;
                ArgumentNullException.ThrowIfNull(connectionStringsOptions, nameof(connectionStringsOptions));

                options.UseSqlServer(!useAzureSql ? connectionStringsOptions.SqlServer : connectionStringsOptions.AzureSqlServer);
            });

            return services;
        }
    }
}
