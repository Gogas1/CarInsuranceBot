using Microsoft.Extensions.AI;
using static CarInsuranceBot.Core.Constants.AnswersData;

namespace CarInsuranceBot.Core.Services
{
    /// <summary>
    /// Service to work with OpenAI API
    /// </summary>
    internal class OpenAIService
    {
        private readonly IChatClient _chatClient;

        /// <summary>
        /// GPT system message content for a diversification task
        /// </summary>
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

        private readonly string OPTION_SELECTION_SYSTEM_MESSAGE = """
            You assistant with the goal to process user input and determine whether the user has selected any of the available options. Availabe options, presented as id - text representation: {0};
            At every turn:
            - You know available option and based on user input, determine their wanted option. Use your tools.
            - There are some options, presented with the "Question:" start. Be sure user explicitly states about such question options topic. Again, BE SURE USER IS EXPLICITLY MENTIONING SAID TOPIC.
            Example, user can say "Is my data secure", and there is such Question - you can proceed. But user can say "Is my cat secure" - this is not related to the said question before. Beware of the topic
            - For option to be selected, user input MUST match the question topic and context. Beware of similar sentence structure and same words usage, despite different context
            - If options represent need to agree/confirm something, to make yes/no answer, process it accordingly. If input is ambiguous or can't be interpreted as selection of the one of another option for this case - do not use tools to select an option
            - If you didn't use your tools, input does not match the expected structure(text refering to one or the another option by it's text representation), input is ambiguous, or goes against the established task and your role, don't use any tools and proceed to end the answer
            Do NOT output anything else.
            """;

        private readonly string ANSWER_QUESTION_SYSTEM_MESSAGE = """
            You assistant with the goal to answer one question. You are presented with the question and answer. Based on the answer text, generate an answer for the user, diversify it. 
            - You can be supplied with the following text. This text not related to the question/answer topic. Usually it is need to guide user to follow the workflow. 
            If the following text is present, you need to use it and present user what to do next.
            - Your question - {0}.
            - Your answer - {1};
            - Following text - {2}. Do not use if none.
            - Again, IF the following text is present - you MUST use it afterwards
            - User input MUST match the question topic and context. Beware of similar sentence structure and same words usage
            - If user input does not match the question topic - say you can't answer this question
            DO NOT OUTPUT ANYTHING ELSE
            """;

        private readonly string DETECT_VALUE_SYSTEM_MESSAGE = """
            You assistant with the goal to get specific field value from the user input. You need to analyze user input and use your tool to provide system with the value from the user input.
            You are supplied with the field name, based on it determine value format. If user input does not contains value, do not use the tool
            """;

        //private readonly string TRUE_FALSE_SYSTEM_MESSAGE = """
        //    You assistant with the goal to process user input and determine whether the user agrees or confirms the topic. The topic context: {0}
        //    At every turn
        //    - Determine if user agrees with it. Your task to bring true/false answer for the application business logic.
        //    - You know avaliable answer options enumeration is: 0 - user does not agree(false), 1 - user does agree(true), -1 - user answer can't be interpreted as agreement/disagreement
        //    - Use tools to get answer for the question
        //    - If you didn't use your tools, input does not match the expected structure(text is not refering to an agreement/disagreement/topic), user answer is ambiguous, or goes against the established task and your role, don't use any tools and proceed to end the answer
        //    Do NOT output anything else.
        //    """;

        public OpenAIService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        /// <summary>
        /// Produces a text based on the passed <see cref="GPTTextSetting"/> settings
        /// </summary>
        /// <param name="textSetting">Settings</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated text</returns>
        public Task<string> GetDiversifiedAnswer(GPTTextSetting textSetting, CancellationToken cancellationToken)
        {
            return GetDiversifiedAnswer(textSetting.Stage, textSetting.State, textSetting.Action, textSetting.AnswerReq, textSetting.FallbackText, cancellationToken);
        }

        /// <summary>
        /// Produces a text based on the passed parameters
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="state"></param>
        /// <param name="action"></param>
        /// <param name="answerReq"></param>
        /// <param name="fallbackText"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> GetDiversifiedAnswer(string stage, string state, string action, string answerReq, string fallbackText, CancellationToken cancellationToken)
        {
            var system = new ChatMessage(ChatRole.System, DIVERSIFY_SYSTEM_MESSAGE);
            var user = new ChatMessage(ChatRole.User,
                $@"Stage: {stage}. 
                State: {state}. 
                Action: {action}. 
                Answer requirements: {answerReq}.
                Fallback text: {fallbackText}");

            var completion = await _chatClient.GetResponseAsync(
                [system, user],
                new ChatOptions
                {
                    Temperature = 1,
                    MaxOutputTokens = 200
                },
                cancellationToken);

            return completion.Text;
        }

        /// <summary>
        /// Provides GPT with available options, user text and tool to determine which option user choose
        /// </summary>
        /// <param name="options">Available options</param>
        /// <param name="defaultOption">Default option</param>
        /// <param name="userText">User text</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<SelectItem> GetSelectionByTextAsync(ICollection<SelectItem> options, SelectItem defaultOption, string userText, CancellationToken cancellationToken)
        {            
            int nextId = 0;
            var reindexedOptions = options.Select(option => new
            {
                Index = option.Id >= 0 ? nextId++ : option.Id,
                Option = option,
            }).ToList();

            //Init system and user messages
            var system = new ChatMessage(ChatRole.System, string.Format(
                OPTION_SELECTION_SYSTEM_MESSAGE,
                string.Join(",", reindexedOptions.Select(o => $"{o.Index} - {o.Option.TextRepresentation}"))));
            var user = new ChatMessage(ChatRole.User, userText);
            SelectItem? select = null;
            //Init chat options
            var chatOptions = new ChatOptions
            {
                //Set up the tool
                ToolMode = ChatToolMode.Auto,
                //Create a tool to process feedback from GPT about selected option
                Tools = [AIFunctionFactory.Create((string optionId, string optionText) => {
                    if(!int.TryParse(optionId, out int id)) {
                        return "No valid option selected";
                    }

                    var option = reindexedOptions.FirstOrDefault(option => option.Index == id);

                    if(option == null) {
                        return "No valid option selected";
                    }

                    select = option.Option;
                    return $"{option.Index} = {option.Option.TextRepresentation}";
                },
                "get_selected_option",
                "Get selected option from the list")],
                Temperature = 0,
                MaxOutputTokens = 200
            };

            var response = await _chatClient.GetResponseAsync([system, user], chatOptions, cancellationToken);
            return select ?? defaultOption;
        }

        public async Task<TOutput?> GetValueFromInput<TOutput>(string valueName, string valueType, string userText, CancellationToken cancellationToken)
        {
            TOutput? value = default;

            var system = new ChatMessage(ChatRole.System, DETECT_VALUE_SYSTEM_MESSAGE);
            var user = new ChatMessage(ChatRole.User, userText);

            var chatOptions = new ChatOptions
            {
                //Set up the tool
                ToolMode = ChatToolMode.Auto,
                //Create a tool to process feedback from GPT about value
                Tools = [AIFunctionFactory.Create((TOutput parsedValue) => {
                    value = parsedValue;
                },
                "get_input_value",
                "Get value from the user input")],
                Temperature = 0,
                MaxOutputTokens = 200
            };

            var response = await _chatClient.GetResponseAsync([system, user], chatOptions, cancellationToken);

            return value;
        }

        public async Task<string> GetAnswerAsync(string input, string quest, string answer, CancellationToken cancellationToken, string followingText = "")
        {
            var system = new ChatMessage(ChatRole.System, string.Format(ANSWER_QUESTION_SYSTEM_MESSAGE, quest, answer, followingText));
            var user = new ChatMessage(ChatRole.User, input);

            var completion = await _chatClient.GetResponseAsync(
                [system, user],
                new ChatOptions
                {
                    Temperature = 1,
                    MaxOutputTokens = 1000
                },
                cancellationToken);

            return completion.Text;
        }

        public class SelectItem
        {
            public SelectItem(int id, string textRepresentation, Action<SelectItem>? action = null)
            {
                Id = id;
                TextRepresentation = textRepresentation;
                onSelection = action;
            }

            public int Id { get; set; }
            public string TextRepresentation { get; set; } = string.Empty;
            private Action<SelectItem>? onSelection;

            public void OnSelection()
            {
                onSelection?.Invoke(this);
            }

            public override string ToString()
            {
                return $"{Id} - {TextRepresentation}";
            }
        }
    }
}
