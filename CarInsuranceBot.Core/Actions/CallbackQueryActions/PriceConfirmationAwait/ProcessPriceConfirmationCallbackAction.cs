using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Cache;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Models.Documents;
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

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions.PriceConfirmationAwait
{
    internal class ProcessPriceConfirmationCallbackAction : ActionBase<CallbackQuery>
    {
        private readonly UserService _userService;
        private readonly ITelegramBotClient _botClient;
        private readonly BotConfiguration _botConfig;
        private readonly MemoryCache _cache;

        private readonly InsuranceService _insuranceService;
        private readonly PdfService _pdfService;

        public ProcessPriceConfirmationCallbackAction(
            UserService userService,
            ITelegramBotClient botClient,
            IOptions<BotConfiguration> botOptions,
            MemoryCache cache,
            InsuranceService insuranceService,
            PdfService pdfService)
        {
            _userService = userService;
            _botClient = botClient;
            _botConfig = botOptions.Value;
            _cache = cache;
            _insuranceService = insuranceService;
            _pdfService = pdfService;
        }

        public override async Task Execute(CallbackQuery update)
        {
            await _botClient.AnswerCallbackQuery(update.Id);

            var data = update.Data;
            if (data == null)
            {
                return;
            }

            if (data == AnswersData.DataConfirmationButtonData)
            {
                await ProcessAgreement(update);
                return;
            }

            if (data == AnswersData.DataDeclineButtonData)
            {
                await ProcessDecline(update);
                return;
            }
        }

        protected virtual async Task ProcessAgreement(CallbackQuery update)
        {
            var insuranceDocumentData = await _insuranceService.CreateInsuranceForUser(update.From.Id);

            if (insuranceDocumentData == null)
            {
                var newNonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));
                var authReq = AnswersData.GetAuthorizationRequestParameters(_botClient, _botConfig, newNonce);

                _cache.Set($"nonce_{update.From.Id}", newNonce, TimeSpan.FromMinutes(10));
                await _botClient.SendMessage(
                    update.From.Id,
                    AnswersData.NoStoredDocumentsText,
                    replyMarkup: InlineKeyboardButton.WithUrl(
                        AnswersData.ShareDocumentButtonText,
                        string.Format(AnswersData.RedirectUrl, authReq.Query)));
                await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, update.From.Id);
                return;
            }

            using var stream = new MemoryStream();
            _pdfService.GenerateInsurancePdf(insuranceDocumentData, stream);
            stream.Position = 0;
            var file = new InputFileStream(stream, "INSURANCE POLICY");
            await _botClient.SendDocument(update.From.Id, file);
        }

        protected virtual async Task ProcessDecline(CallbackQuery update)
        {
            await _botClient.SendMessage(update.From.Id, AnswersData.FirstPriceDeclineText, replyMarkup: AnswersData.PriceConfirmationKeyboard);
            await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PriceSecondConfirmationAwait, update.From.Id);
        }
    }
}
