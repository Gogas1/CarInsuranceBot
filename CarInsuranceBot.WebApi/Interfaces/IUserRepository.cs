using CarInsuranceBot.Core.Interfaces.Repositories;

namespace CarInsuranceBot.WebApi.Interfaces
{
    /// <summary>
    /// User repository for the web app needs. Implements telegram bot <see cref="ITelegramBotUserRepository"/> user repository
    /// </summary>
    public interface IUserRepository : ITelegramBotUserRepository
    {

    }
}
