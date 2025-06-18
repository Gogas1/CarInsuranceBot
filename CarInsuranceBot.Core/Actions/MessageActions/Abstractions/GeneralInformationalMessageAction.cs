using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Actions.MessageActions.Abstractions
{
    internal abstract class GeneralInformationalMessageAction : MessageActionBase
    {
        private OpenAIService _openAiService;
        private List<OpenAIService.SelectItem> _injectVariants = new();

        private string guidanceInstructions = "";
        private ReplyMarkup? replyMarkup = null;

        protected GeneralInformationalMessageAction(
            UserService userService,
            ITelegramBotClient botClient,
            OpenAIService openAiService) : base(userService, botClient)
        {
            
            _openAiService = openAiService;
        }

        protected override Task ProcessLogicAsync(Message update, CancellationToken cancellationToken)
        {
            _injectVariants.AddRange([
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
                ]);

            return Task.CompletedTask;
        }        

        protected void InjectQuestionsOptions(ICollection<OpenAIService.SelectItem> options, string guidanceInstructions, ReplyMarkup? replyMarkup = null)
        {
            this.guidanceInstructions = guidanceInstructions;
            this.replyMarkup = replyMarkup;

            foreach (var item in _injectVariants)
            {
                options.Add(item);
            }
        }

        private async Task AnswerQuestion(Message update, string? input, string question, string answer, string guidanceInstructions, CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(input))
            {
                return;
            }

            var gptAnswer = await _openAiService.GetAnswerAsync(input, question, answer, cancellationToken, guidanceInstructions);                        
            await _botClient.SendMessage(update.Chat, gptAnswer, replyMarkup: replyMarkup, cancellationToken: cancellationToken);

        }
    }
}
