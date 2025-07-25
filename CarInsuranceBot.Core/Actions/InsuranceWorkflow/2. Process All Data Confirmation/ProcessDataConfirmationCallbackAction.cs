﻿using CarInsuranceBot.Core.Actions.Abstractions;
using CarInsuranceBot.Core.Configuration;
using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Enums;
using CarInsuranceBot.Core.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Core.Actions.CallbackQueryActions
{
    /// <summary>
    /// <see cref="CallbackQueryActionBase"/> implementation action to process user agreement with extracted documents data corectness. 
    /// <para>Exit states: <see cref="Enums.UserState.PriceConfirmationAwait"/> if user agree, <see cref="Enums.UserState.DocumentsAwait"/> if disagree and need to resubmit the documents</para>
    /// </summary>
    internal class ProcessDataConfirmationCallbackAction : CallbackQueryActionBase
    {

        private readonly BotConfiguration _botConfig;
        private readonly OpenAIService _openAIService;
        private readonly DocumentsService _documentsService;

        public ProcessDataConfirmationCallbackAction(
            UserService userService,
            ITelegramBotClient botClient,
            IOptions<BotConfiguration> botOptions,
            OpenAIService openAIService,
            DocumentsService documentsService) : base(userService, botClient)
        {
            _botConfig = botOptions.Value;
            _openAIService = openAIService;
            _documentsService = documentsService;
        }

        protected override async Task ProcessLogicAsync(CallbackQuery update, CancellationToken cancellationToken)
        {
            //await _botClient.AnswerCallbackQuery(update.Id);

            var data = update.Data;
            if (data == null)
            {
                return;
            }

            switch(data)
            {
                // If callback query contains agreement data
                case AnswersData.DATA_CONFIRMATION_BUTTON_DATA:
                    // Send message about the next step - price agreement text and keyboard
                    await _botClient.SendMessage(
                        update.From.Id,
                        await _openAIService.GetDiversifiedAnswer(AnswersData.DATA_CONFIRMED_SETTINGS, cancellationToken),
                        replyMarkup: AnswersData.PRICE_CONFIRMATION_KEYBOARD,
                        cancellationToken: cancellationToken);
                    // Change user state
                    await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.PriceConfirmationAwait, update.From.Id, cancellationToken);
                    return;
                // If callback query contains decline data
                case AnswersData.DATA_DECLINE_BUTTON_DATA:
                    // Create and set new nonce
                    var newNonce = _documentsService.SetNonceForUser(update.From.Id);

                    // Send message about resubmitting the documents and authorization keyboard
                    await _botClient.SendMessage(
                        update.From.Id,
                        await _openAIService.GetDiversifiedAnswer(AnswersData.DATA_DECLINED_SETTINGS, cancellationToken),
                        replyMarkup: AnswersData.GetAuthorizationKeyboard(_botClient, _botConfig, newNonce));
                    // Change user state
                    await _userService.SetUserStateByTelegramIdAsync(Enums.UserState.DocumentsAwait, update.From.Id, cancellationToken);
                    return;
                case AnswersData.STOP_WORKFLOW_BUTTON_DATA:
                    await _botClient.SendMessage(
                    update.From.Id,
                    AnswersData.USER_RECONSIDERED_ANSWER_TEXT,
                    replyMarkup: AnswersData.HOME_KEYBOARD,
                    cancellationToken: cancellationToken);
                        await _userService.SetUserStateByTelegramIdAsync(UserState.Home, update.From.Id, cancellationToken);
                    return;
            }
        }
    }
}
