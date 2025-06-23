using CarInsuranceBot.Core.Configuration;
using System.Globalization;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Constants
{
    /// <summary>
    /// Data for the bot answers
    /// </summary>
    internal static class AnswersData
    {
        /// <summary>
        /// GPT model request data model
        /// </summary>
        internal record GPTTextSetting
        {
            public string Stage { get; set; } = string.Empty;
            public string State { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string AnswerReq { get; set; } = string.Empty;
            public string FallbackText { get; set; } = string.Empty;
        }


        #region HelloMessageAction
                
        //Fallback text for the welcoming message
        public static readonly string HELLO_MESSAGE_FALLBACK_TEXT = "Hello. I am a car insurance bot. I can assist you with car insurance purchases";
        public static readonly GPTTextSetting HELLO_MESSAGE_SETTINGS = new()
        {
            Stage = "No stage",
            State = "first interaction",
            Action = "User initiated first contact with bot.",
            AnswerReq = "Write you are a car insurance bot, and your task is to assist user wiht car insurance purchases",
            FallbackText = HELLO_MESSAGE_FALLBACK_TEXT,
        };

        #endregion HelloMessageAction


        #region ProcessReconsiderationAction

        // Text for user reconsideration about ordering an insurance
        public static readonly string USER_RECONSIDERED_ANSWER_TEXT = "Got it. Returning to the start.";

        #endregion ProcessReconsiderationAction


        #region BusyHandlingAction

        // Text for notifying an user about other running task for them
        public static readonly string BOT_BUSY_FOR_USER_ANSWER_TEXT = "I am processing other task for you. Please, wait for completion of the last task.";

        #endregion BusyHandlingAction


        #region DefaultHomeMessage

        // Home message GPT request settings data
        public static readonly string HOME_FALLBACK_TEXT = "Proceed with available actions";
        public static readonly string HOME_STAGE = "Basic \"Home\" state";
        public static readonly string HOME_STATE = "Get basic availalble operations";
        public static readonly string HOME_ACTION = "User has interacted with the bot, having no state or active workflow";
        public static readonly string HOME_ANSWER_REQ = "Write a message offering user to proceed with one of the available actions. Don't list the actions.";

        public static readonly GPTTextSetting HOME_MESSAGE_SETTINGS = new()
        {
            Stage = HOME_STAGE,
            State = HOME_STATE,
            Action = HOME_ACTION,
            AnswerReq = HOME_ANSWER_REQ,
            FallbackText = HOME_FALLBACK_TEXT,
        };

        public static readonly string GET_INSURANCE_BUTTON_TEXT = "Get Insurance";
        public static readonly InlineKeyboardButton[] HOME_KEYBOARD = [new InlineKeyboardButton(GET_INSURANCE_BUTTON_TEXT, "get_insurance")];

        #endregion DefaultHomeMessage

        #region Cancellation shared

        // Timeout message
        public static readonly string TIMEOUT_ANSWER_TEXT = "Sorry, your request took too long. Please try again.";

        #endregion Cancellation shared


        #region InitCreateInsuranceFlow

        // Message fallback text for insurance ordering workflow start
        public static readonly string START_INSURANCE_WORKFLOW_FALLBACK_TEXT = "Starting getting an insurance for you. Please, provide a photo of your ID and driver license.";
        public static readonly GPTTextSetting START_INSURANCE_WORKFLOW_SETTINGS = new()
        {
            Stage = "Insurance workflow",
            State = "user started insurance ordering workflow",
            Action = "User started insurance ordering workflow.",
            AnswerReq = "Write about starting getting insurance for user and ask to share a photos of ID and driver license via authorization system",
            FallbackText = START_INSURANCE_WORKFLOW_FALLBACK_TEXT,
        };

        #endregion InitCreateInsuranceFlow


        #region Authorization Shared

        public static readonly string SHARE_DOCUMENTS_IN_CHAT_BUTTON_TEXT = "Share via chat";
        public static readonly string SHARE_DOCUMENTS_IN_CHAT_BUTTON_DATA = "share_in_chat";

        public static readonly string AUTHORIZATION_DECLINE_BUTTON_TEXT = "I have reconsidered this";
        public static readonly string AUTHORIZATION_DECLINE_BUTTON_DATA = "decline";

        public static readonly string SHARE_DOCUMENTS_BUTTON_TEXT = "Share via Telegram Passport";
        public static readonly string REDIRECT_URL = "https://gogas1.github.io/CarInsuranceBot/redirect.html?{0}";

        public static readonly string SHARE_PASSPORT_IN_CHAT_FALLBACK_TEXT = "Good. Share your passport photo in chat";
        public static readonly GPTTextSetting SHARE_PASSPORT_IN_CHAT_GPT_SETTINGS = new()
        {
            Stage = "Insurance workflow",
            State = "need to provide passport photo",
            Action = "User decided to share their documents via chat",
            AnswerReq = "Write about need to share photo of their passport in chat",
            FallbackText = SHARE_PASSPORT_IN_CHAT_FALLBACK_TEXT

        };

        public static readonly string SHARE_LICENSE_IN_CHAT_FALLBACK_TEXT = "Good. Share your driving license photo in chat";
        public static readonly GPTTextSetting SHARE_LICENSE_IN_CHAT_GPT_SETTINGS = new()
        {
            Stage = "Insurance workflow",
            State = "need to provide driving license photo",
            Action = "User submitted passport photo and went to the next step",
            AnswerReq = "Write about success of submitting passport photo and need to share photo of their driving license in chat",
            FallbackText = SHARE_LICENSE_IN_CHAT_FALLBACK_TEXT

        };

        public static readonly string INSURANCE_ORDERING_DOCUMENTS_IN_CHAT_OPTION = "User want to proceed documents processing using chat. Want to share documents in chat";
        public static readonly string INSURANCE_ORDERING_RECONSIDERATION_OPTION = "User don't want to proceed with insurance. Want go back to the home state. Decline the workflow.";

        public static readonly string STOP_WORKFLOW_BUTTON_TEXT = "Back to home";
        public static readonly string STOP_WORKFLOW_BUTTON_DATA = "workflow_stop";
        public static InlineKeyboardButton[] STOP_WORKFLOW_KEYBOARD = [
            new InlineKeyboardButton(STOP_WORKFLOW_BUTTON_TEXT, STOP_WORKFLOW_BUTTON_DATA)
            ];

        private static AuthorizationRequestParameters GetAuthorizationRequestParameters(ITelegramBotClient botClient, BotConfiguration botConfig, string nonce)
        {
            // Init and return telegram authorization parameters to request passport and driver licenze access
            return new AuthorizationRequestParameters(
                botClient.BotId,
                botConfig.Public256Key,
                nonce,
                new PassportScope
                {
                    Data = new[]
                    {
                new PassportScopeElementOne(EncryptedPassportElementType.Passport),
                new PassportScopeElementOne(EncryptedPassportElementType.DriverLicense)
                    }
                });
        }

        private static readonly CompositeFormat _redirectUrlFormat = CompositeFormat.Parse(AnswersData.REDIRECT_URL);
        public static InlineKeyboardButton[][] GetAuthorizationKeyboard(ITelegramBotClient client, BotConfiguration botConfig, string nonce)
        {
            // Get auth parameters
            var authReq = GetAuthorizationRequestParameters(client, botConfig, nonce);


            //Construct inline keyboard
            InlineKeyboardButton[][] keyboard = [
                [
                    InlineKeyboardButton.WithUrl(
                        AnswersData.SHARE_DOCUMENTS_BUTTON_TEXT,
                        string.Format(CultureInfo.InvariantCulture, _redirectUrlFormat, authReq.Query)),
                    new InlineKeyboardButton(SHARE_DOCUMENTS_IN_CHAT_BUTTON_TEXT, SHARE_DOCUMENTS_IN_CHAT_BUTTON_DATA)
                ],
                [new InlineKeyboardButton(AUTHORIZATION_DECLINE_BUTTON_TEXT, AUTHORIZATION_DECLINE_BUTTON_DATA)]
                ];
            return keyboard;
        }

        #endregion Authorization Shared

        #region ProcessDocumentsDataAction

        // Start processing user data fallback text
        public static readonly string START_PROCESSING_FALLBACK_TEXT = "Your data has been received. We are processing it.";
        public static readonly GPTTextSetting START_PROCESSING_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "application received user documents",
            Action = "User sent the documents using application authorization workflow system",
            AnswerReq = "Write we have received user's data and it is under processing",
            FallbackText = START_PROCESSING_FALLBACK_TEXT,
        };

        // No documents provided fallback text
        public static readonly string NO_DOCUMENTS_PROVIDED_FALLBACK_TEXT = "You have not provided documents. Please try again";
        public static readonly GPTTextSetting NO_DOCUMENTS_PROVIDED_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "application failed to receive user documents",
            Action = "User failed to send documents using application authorization workflow system",
            AnswerReq = "Write we have not received data from the user, so they need to try again. Prompt to use the authorization system vie the message keyboard buttons in case they weren't used in first place, as they is the only option",
            FallbackText = NO_DOCUMENTS_PROVIDED_FALLBACK_TEXT,
        };

        // No credentials in the documents text
        public static readonly string NO_CREDENTIALS_FALLLBACK_TEXT = "Credentials from the passport data are missing. Please try again";
        public static readonly GPTTextSetting NO_CREDENTIALS_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "application failed to get credentials from user data",
            Action = "User sent the documents using application authorization workflow system, but application failed to get credentials from the authorization system",
            AnswerReq = "Write we have failed to get credentials data from the user data, so user need to try again",
            FallbackText = NO_CREDENTIALS_FALLLBACK_TEXT,
        };

        // No nonce text
        public static readonly string NO_NONCE_TEXT = "Authorization data have been expired. Please resubmit your data again";

        // No documents data text for telegram passport error text
        public static readonly string NO_DOCUMENTS_DATA_TEXT = "Cannot extract document data, try make photo again";

        //No documents data message fallback text
        public static readonly string NO_DOCUMENT_DATA_FALLBACK_TEXT = "We couldn't extract data from some of the provided documents. Please try again.";
        public static readonly GPTTextSetting NO_DOCUMENT_DATA_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "application failed to get data from the data extraction api",
            Action = "Application has received answer from the data extraction api, but it failed to extract info from some of the documents",
            AnswerReq = "Write we have failed to extract data from some of the user's document, so user need to try again and be sure they submit correct document",
            FallbackText = NO_DOCUMENT_DATA_FALLBACK_TEXT,
        };

        // Documents data confirmation message text
        public static readonly string DOCUMENTS_PROVIDED_TEXT = """
            Validate extracted documents data and confirm if it is correct.
            Passport:
            Number - {0};
            Country code - {1};
            Surname - {2};
            Name - {3};
            Birth date - {4};
            Expiry date - {5};
            Driving license:
            Id - {6};
            Country code - {7};
            Category - {8}
            """;

        #endregion ProcessDocumentsDataAction

        #region Data confirmation shared

        //Data confirmation keyboard buttons content
        public static readonly string NO_CONCRETE_ANSWER_FALLBACK_TEXT = "You did not provide concrete answer. For more direct workflow, please use message keyboard buttons";

        public static readonly string DATA_CONFIRMATION_BUTTON_TEXT = "Yes, I confirm";
        public static readonly string DATA_DECLINE_BUTTON_TEXT = "No, data is incorrect";
        public static readonly string DATA_CONFIRMATION_BUTTON_DATA = "yes";
        public static readonly string DATA_DECLINE_BUTTON_DATA = "no";

        // Data confirmation keyboard
        public static readonly InlineKeyboardButton[] DATA_CONFIRMATION_KEYBOARD = [
                new InlineKeyboardButton(DATA_CONFIRMATION_BUTTON_TEXT, DATA_CONFIRMATION_BUTTON_DATA),
                new InlineKeyboardButton(DATA_DECLINE_BUTTON_TEXT, DATA_DECLINE_BUTTON_DATA)
                ];

        #endregion Data confirmation shared        

        #region Price agreement shared

        public static readonly string MUST_USE_KEYBOARD_TEXT = "You need to use message keyboard to agree or disagree with price.";

        public static readonly string NO_STORED_DOCUMENTS_FALLLBACK_TEXT = "Your information is no longer stored. Resubmit it please.";
        public static readonly GPTTextSetting NO_STORED_DOCUMENTS_SETTINGS = new()
        {
            Stage = "Agreement processing",
            State = "application no longer storing user info",
            Action = "Application tried to use user's data, but the data is missing",
            AnswerReq = "Write user's documents data is no longer stored, so they need to resubmit it",
            FallbackText = NO_STORED_DOCUMENTS_FALLLBACK_TEXT,
        };

        public static readonly string INSURANCE_GRANTED_FALLBACK_TEXT = "Your insurance has been ordered. Keep the provided document as a confirmaion.";
        public static readonly GPTTextSetting INSURANCE_GRANTED_SETTINGS = new()
        {
            Stage = "Insurance processing",
            State = "application has ordered user's insurance",
            Action = "Application has successfully ordered insurance for user and sent a confirmation document for them",
            AnswerReq = "Write we has ordered insurance for the user and they need to keep provided document as a confirmation",
            FallbackText = INSURANCE_GRANTED_FALLBACK_TEXT,
        };

        public static readonly string FIRST_PRICE_DECLINE_FALLBACK_TEXT = "We are sorry, the price 100 US dollars is only available price. Do you agree with this price?";
        public static readonly GPTTextSetting FIRST_PRICE_DECLINE_SETTINGS = new()
        {
            Stage = "Price agreement processing",
            State = "user declined the price once",
            Action = "User declined the proposed 100 dollars price",
            AnswerReq = "Write that we are sorry, but state 100 US dollars price is the single available price. Reask about agreement with such price",
            FallbackText = FIRST_PRICE_DECLINE_FALLBACK_TEXT,
        };

        public static readonly string SECOND_PRICE_DECLINE_FALLBACK_TEXT = "Got it. Cancelling insurance ordering workflow.";
        public static readonly GPTTextSetting SECOND_PRICE_DECLINE_SETTINGS = new()
        {
            Stage = "Price agreement processing",
            State = "user declined the price twice",
            Action = "User declined the proposed 100 dollars price second time",
            AnswerReq = "Since user declined price twice, write you have got it, cancelling insurance ordering workflow, and offer user to proceed with one of the available bot actions",
            FallbackText = SECOND_PRICE_DECLINE_FALLBACK_TEXT,
        };

        public static readonly string PRICE_CONFIRMATION_BUTTON_TEXT = "Yes, I agree";
        public static readonly string PRICE_DECLINE_BUTTON_TEXT = "No, I don't agree";
        public static readonly string PRICE_CONFIRMATION_BUTTON_DATA = "yes";
        public static readonly string PRICE_DECLINE_BUTTON_DATA = "no";

        public static readonly InlineKeyboardButton[] PRICE_CONFIRMATION_KEYBOARD = [
                new InlineKeyboardButton(PRICE_CONFIRMATION_BUTTON_TEXT, PRICE_CONFIRMATION_BUTTON_DATA),
                new InlineKeyboardButton(PRICE_DECLINE_BUTTON_TEXT, PRICE_DECLINE_BUTTON_DATA)
                ];

        #endregion Price agreement shared

        #region ProcessDataConfirmationAction

        // Data corectness declined and user need to resubmit documents message fallback text
        public static readonly string DATA_DECLINED_FALLBACK_TEXT = "Please resubmit your documents.";
        public static readonly GPTTextSetting DATA_DECLINED_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "documents correctness declined",
            Action = "User got asked about correctness of the extracted documents data and declined it",
            AnswerReq = "Write about resubmitting user's documents",
            FallbackText = DATA_DECLINED_FALLBACK_TEXT,
        };

        // Data is correct, proceed to the price confirmation flow fallback text
        public static readonly string DATA_CONFIRMED_FALLBACK_TEXT = "Thanks. Insurance fixed price is 100 US dollars. Are you agreeing with this price and ready to proceed?";
        public static readonly GPTTextSetting DATA_CONFIRMED_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "documents correctness confirmed",
            Action = "User got asked about correctness of the extracted documents data and confirmed it",
            AnswerReq = "Thank user. Offer insurance 100 US dollars fixed price and ask about agreement with such price",
            FallbackText = DATA_CONFIRMED_FALLBACK_TEXT,
        };

        #endregion ProcessDataConfirmationAction

        #region Side questions Shared

        public static readonly string FOR_WHAT_QUESTION = "Question: For what? For what you need my data?";
        public static readonly string FOR_WHAT_ANSWER = "Answer: We need your data for car insurance ordering process";

        public static readonly string HOW_MY_DATA_IS_STORED_QUESTION = "Question: How my(user's) data is stored?";
        public static readonly string HOW_MY_DATA_IS_STORED_ANSWER = "Answer: Users data stored for short time and encrypted. It is disposed after processing?";

        public static readonly string IS_MY_DATA_SAFE_QUESTION = "Question: Is my(user's) data safe? Is it stored securely?";
        public static readonly string IS_MY_DATA_SAFE_ANSWER = @"Answer: User's data encrypted and stored for the short time. We don't store user files and generated documents on a server. 
            If user want to secure data on the telegram side - they can use Telegram Passport system. It will additionaly secure data.";

        public static readonly string WHAT_DO_YOU_STORE_QUESTION = "Question: What data do you store?";
        public static readonly string WHAT_DO_YOU_STORE_ANSWER = @"Answer: Documents data stored for one hour maximum - we need to access it during insurance ordering workflow. 
            It is immidiately purged on the exit from the workflow. For the long time we are storing your telegram Id to handle workflow state between messages";

        public static readonly string WHO_HAS_ACCESS_QUESTION = "Question: Who has access to my data?";
        public static readonly string WHO_HAS_ACCESS_ANSWER = "Answer: Only Mindee services has access to perform data extraction. No other third party services don't need and don't have access to the data of this bot.";

        public static readonly string HOW_LONG_DATA_STORED_QUESTION = "Question: How long my(user's) documents data is stored";
        public static readonly string HOW_LONG_DATA_STORED_ANSWER = "Answer: Your documents data is stored for one hour at max, and deleted on the insurance workflow exit";

        public static readonly string HOW_MY_DATA_USED_QUESTION = "Question: How my(user's) data is used. Do you use it for X?";
        public static readonly string HOW_MY_DATA_USED_ANSWER = @"Answer: Your documents photos is used to extract your data, validate insurance availability and to fill up document we send you. 
            We use Mindee services to extract data from the photos. Your data is not used in any other requests to the services.";

        public static readonly string HOW_IT_IS_SECURED_QUESTION = "Question: What safeguards are in place against unauthorized access or data breaches?";
        public static readonly string HOW_IT_IS_SECURED_ANSWER = "Answer: Your data is encrypted. Encryption keys are stored separately from the data. Your data stored for short time";

        public static readonly string I_DONT_WANT_TO_GIVE_DATA_QUESTION = "Question: What if I don't want to share my private data?";
        public static readonly string I_DONT_WANT_TO_GIVE_DATA_ANSWER = "Answer: Tell we are sorry, but we need to get access to your data for the service we provide. User data is secured during the process.";

        #endregion Side questions Shared

        #region Guidance answers

        //DefaultHomeMessage
        public static readonly string HOME_STATE_GUIDANCE = "Answer to the user they on this step need to select one of the available bot actions.";

        //PassportProcessingMessageAction
        public static readonly string PASSPORT_AWAIT_STATE_GUIDANCE = "Answer to the user they need to submit photo of their passport.";

        //LicenseProcessingMessageAction
        public static readonly string LICENSE_AWAIT_STATE_GUIDANCE = "Answer to the user they need to submit photo of their driving license.";

        //ProcessDocumentsDataAction
        public static readonly string DOCUMENTS_AWAIT_STATE_GUIDANCE = "Answer to the user they need use Telegram Passport or choose to share documents via chat to proceed.";

        //ProcessDataConfirmationMessageAction
        public static readonly string DATA_CONFIRMATION_AWAIT_STATE_GUIDANCE = "Answer to the user they need to confirm or deconfirm provided data corectness.";

        #endregion Guidance answers        
    }
}
