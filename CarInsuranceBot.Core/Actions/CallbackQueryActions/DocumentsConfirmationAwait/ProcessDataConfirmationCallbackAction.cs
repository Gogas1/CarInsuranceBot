using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.DocumentsConfirmationAwait
{
    internal class ProcessDataConfirmationCallbackAction : ActionBase<CallbackQuery>
    {
        private readonly UserService _userService;
        private readonly ITelegramBotClient _botClient;
        private readonly BotConfiguration _botConfig;
        private readonly MemoryCache _cache;

        public ProcessDataConfirmationCallbackAction(UserService userService, ITelegramBotClient botClient, IOptions<BotConfiguration> botOptions, MemoryCache cache)
        {
            _userService = userService;
            _botClient = botClient;
            _botConfig = botOptions.Value;
            _cache = cache;
        }

        public override async Task Execute(CallbackQuery update)
        {
            await _botClient.AnswerCallbackQuery(update.Id);
            
            var data = update.Data;
            if(data == null)
            {
                return;
            }

            if (data == AnswersData.DataConfirmationButtonData)
            {
                await _botClient.SendMessage(update.From.Id, AnswersData.DataConfirmedFallbackText, replyMarkup: AnswersData.PriceConfirmationKeyboard);
                await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PriceConfirmationAwait, update.From.Id);
                return;
            }

            if (data == AnswersData.DataDeclineButtonData)
            {
                var newNonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));
                var authReq = AnswersData.GetAuthorizationRequestParameters(_botClient, _botConfig, newNonce);

                _cache.Set($"nonce_{update.From.Id}", newNonce, TimeSpan.FromMinutes(10));
                await _botClient.SendMessage(
                    update.From.Id,
                    AnswersData.DataDeclinedFallbackText,
                    replyMarkup: InlineKeyboardButton.WithUrl(
                        AnswersData.ShareDocumentButtonText,
                        string.Format(AnswersData.RedirectUrl, authReq.Query)));
                await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, update.From.Id);
                return;
            }
        }
    }
}
