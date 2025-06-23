using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Extensions;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Actions.Abstractions
{
    internal abstract class GeneralInformationalMessageAction : MessageActionBase
    {
        protected OpenAIService _openAiService;

        private List<OpenAIService.SelectItem> _injectVariants = new();

        private string guidanceInstructions = "";
        private ReplyMarkup? replyMarkup;

        protected GeneralInformationalMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAiService) : base(userService, botClient)
        {
            
            _openAiService = openAiService;
        }       

        protected List<OpenAIService.SelectItem> CreateBaseQuestionsOptionsList(Message update, string guidanceInstructions, ReplyMarkup? replyMarkup = null, CancellationToken cancellationToken = default)
        {
            this.guidanceInstructions = guidanceInstructions;
            this.replyMarkup = replyMarkup;

            return [
                new OpenAIService.SelectItem(
                    0,
                    AnswersData.FOR_WHAT_QUESTION,
                    async _ => await AnswerQuestion(
                        update,
                        update.Text,
                        AnswersData.FOR_WHAT_QUESTION,
                        AnswersData.FOR_WHAT_ANSWER,
                        guidanceInstructions,
                        cancellationToken)),
                new OpenAIService.SelectItem(
                    0,
                    AnswersData.HOW_MY_DATA_IS_STORED_QUESTION,
                    async _ => await AnswerQuestion(
                        update,
                        update.Text,
                        AnswersData.HOW_MY_DATA_IS_STORED_QUESTION,
                        AnswersData.HOW_MY_DATA_IS_STORED_ANSWER,
                        guidanceInstructions,
                        cancellationToken)),
                new OpenAIService.SelectItem(
                    0,
                    AnswersData.IS_MY_DATA_SAFE_QUESTION,
                    async _ => await AnswerQuestion(
                        update,
                        update.Text,
                        AnswersData.IS_MY_DATA_SAFE_QUESTION,
                        AnswersData.IS_MY_DATA_SAFE_ANSWER,
                        guidanceInstructions,
                        cancellationToken)),
                new OpenAIService.SelectItem(
                    0,
                    AnswersData.WHAT_DO_YOU_STORE_QUESTION,
                    async _ => await AnswerQuestion(
                        update,
                        update.Text,
                        AnswersData.WHAT_DO_YOU_STORE_QUESTION,
                        AnswersData.WHAT_DO_YOU_STORE_ANSWER,
                        guidanceInstructions,
                        cancellationToken)),
                new OpenAIService.SelectItem(
                    0,
                    AnswersData.WHO_HAS_ACCESS_QUESTION,
                    async _ => await AnswerQuestion(
                        update,
                        update.Text,
                        AnswersData.WHO_HAS_ACCESS_QUESTION,
                        AnswersData.WHO_HAS_ACCESS_ANSWER,
                        guidanceInstructions,
                        cancellationToken)),
                new OpenAIService.SelectItem(
                    0,
                    AnswersData.HOW_MY_DATA_USED_QUESTION,
                    async _ => await AnswerQuestion(
                        update,
                        update.Text,
                        AnswersData.HOW_MY_DATA_USED_QUESTION,
                        AnswersData.HOW_MY_DATA_USED_ANSWER,
                        guidanceInstructions,
                        cancellationToken)),
                new OpenAIService.SelectItem(
                    0,
                    AnswersData.HOW_IT_IS_SECURED_QUESTION,
                    async _ => await AnswerQuestion(
                        update,
                        update.Text,
                        AnswersData.HOW_IT_IS_SECURED_QUESTION,
                        AnswersData.HOW_IT_IS_SECURED_ANSWER,
                        guidanceInstructions,
                        cancellationToken)),
                new OpenAIService.SelectItem(
                    0,
                    AnswersData.I_DONT_WANT_TO_GIVE_DATA_QUESTION,
                    async _ => await AnswerQuestion(
                        update,
                        update.Text,
                        AnswersData.I_DONT_WANT_TO_GIVE_DATA_QUESTION,
                        AnswersData.I_DONT_WANT_TO_GIVE_DATA_ANSWER,
                        guidanceInstructions,
                        cancellationToken))];
        }

        private async Task AnswerQuestion(Message update, string? input, string question, string answer, string guidanceInstructions, CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(input))
            {
                return;
            }

            var gptAnswer = await _openAiService.GetAnswerAsync(input.Truncate(200), question, answer, cancellationToken, guidanceInstructions);                        
            await _botClient.SendMessage(update.Chat, gptAnswer, replyMarkup: replyMarkup, cancellationToken: cancellationToken);

        }
    }
}
