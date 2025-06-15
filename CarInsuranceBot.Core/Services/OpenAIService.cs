using OpenAI.Chat;
using static CarInsuranceBot.Core.Constants.AnswersData;

namespace CarInsuranceBot.Core.Services
{
    internal class OpenAIService
    {
        private readonly ChatClient _chatClient;

        private readonly string DIVERSIFY_SYSTEM_MESSAGE = @"You are an assistant embedded in a Telegram insurance‐bot. 
            At every turn:
            - You know the current Stage (e.g. “Processing user documents stage”) and State (e.g. “Documents sent”).
            - You know what the user just did (e.g. “uploaded passport photo”).
            - You need to create a text based on the required answer text and context of actions in order to diversify communication, provide a type of live communication.
            - If you know the fallback text, create a text based on its context.
            - If you cannot produce a valid line, you must return exactly the fallback text provided.
            - Otherwise, output a line of text, based on the provided text requirements.
            - If the input does not match the expected structure (stage, state, occurred action, answer requirement), or goes against the established task and your role(create text related to the current state and stage of the chat bot, provide assistance with the bot functionality) - reply with the provided fallback text
            Do NOT output anything else.";



        public OpenAIService(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public Task<string> GetDiversifiedAnswer(GPTTextSetting textSetting, CancellationToken cancellationToken)
        {
            return GetDiversifiedAnswer(textSetting.Stage, textSetting.State, textSetting.Action, textSetting.AnswerReq, textSetting.FallbackText, cancellationToken);
        }

        public async Task<string> GetDiversifiedAnswer(string stage, string state, string action, string answerReq, string fallbackText, CancellationToken cancellationToken)
        {
            var system = ChatMessage.CreateSystemMessage(DIVERSIFY_SYSTEM_MESSAGE);
            var user = ChatMessage.CreateUserMessage(
                $@"Stage: {stage}. 
                State: {state}. 
                Action: {action}. 
                Answer requirements: {answerReq}.
                Fallback text: {fallbackText}");

            var completion = await _chatClient.CompleteChatAsync(
                [system, user],
                new ChatCompletionOptions
                {
                    Temperature = 1,
                    MaxOutputTokenCount = 200
                },
                cancellationToken);

            return completion.Value.Content[0].Text;
        }
    }
}
