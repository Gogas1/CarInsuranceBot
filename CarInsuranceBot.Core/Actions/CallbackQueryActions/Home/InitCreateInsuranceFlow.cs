using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.Home
{
    internal class InitCreateInsuranceFlow : ActionBase<CallbackQuery>
    {
        private readonly UserService _userService;
        private readonly ITelegramBotClient _botClient;
        private readonly BotConfiguration _botConfig;
        private readonly MemoryCache _cache;

        private string FallbackMessageText = "Starting getting an insurance for you. Please, provide a photo of your ID and driver license.";

        public InitCreateInsuranceFlow(UserService userService, ITelegramBotClient botClient, IOptions<BotConfiguration> botOptions, MemoryCache cache)
        {
            _userService = userService;
            _botClient = botClient;
            _botConfig = botOptions.Value;
            _cache = cache;
        }

        public override async Task Execute(CallbackQuery update)
        {
            await _botClient.AnswerCallbackQuery(update.Id, FallbackMessageText);
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));
            AuthorizationRequestParameters authReq = new AuthorizationRequestParameters(
                botId: _botClient.BotId,
                publicKey: _botConfig.Public256Key,
                nonce: nonce,
                scope: new PassportScope
                {
                    Data = [
                        new PassportScopeElementOne(EncryptedPassportElementType.Passport),
                        new PassportScopeElementOne(EncryptedPassportElementType.DriverLicense)
                        ]
                }
                );

            await _botClient.SendMessage(
                update.From.Id, 
                FallbackMessageText,                
                replyMarkup: InlineKeyboardButton.WithUrl("Share via Passport", $"https://gogas1.github.io/CarInsuranceBot/redirect.html?{authReq.Query}"));
            _cache.Set(update.From.Id, nonce, TimeSpan.FromMinutes(10));
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, update.From.Id);

        }
    }
}
