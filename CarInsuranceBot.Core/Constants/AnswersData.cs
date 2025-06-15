using CarInsuranceBot.Core.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Constants
{
    internal static class AnswersData
    {
        internal record GPTTextSetting
        {
            public string Stage { get; set; } = string.Empty;
            public string State { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string AnswerReq { get; set; } = string.Empty;
            public string FallbackText { get; set; } = string.Empty;
        }

        // HelloMessageAction
        public static readonly string HELLO_MESSAGE_FALLBACK_TEXT = "Hello. I am a car insurance bot. I can assist you with car insurance purchases";
        public static readonly GPTTextSetting HELLO_MESSAGE_SETTINGS = new()
        {
            Stage = "No stage",
            State = "first interaction",
            Action = "User initiated first contact with bot.",
            AnswerReq = "Write you are a car insurance bot, and your task is to assist user wiht car insurance purchases",
            FallbackText = HELLO_MESSAGE_FALLBACK_TEXT,
        };

        // ProcessReconsiderationAction
        public static readonly string USER_RECONSIDERED_ANSWER_TEXT = "Got it. Returning to the start.";

        // BusyHandlingAction
        public static readonly string BOT_BUSY_FOR_USER_ANSWER_TEXT = "I am processing other task for you. Please, wait for completion of the last task.";

        // DefaultHomeMessage
        public static readonly string HOME_FALLBACK_TEXT = "Proceed with available actions";
        public static readonly string HOME_STAGE = "Basic \"Home\" state";
        public static readonly string HOME_STATE = "Get basic availalble operations";
        public static readonly string HOME_ACTION = "User has interacted with bot, having no state or active workflow";
        public static readonly string HOME_ANSWER_REQ = "Write a message offering user to proceed with one of the available actions";

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

        //Cancellation shared
        public static readonly string TIMEOUT_ANSWER_TEXT = "Sorry, your request took too long. Please try again.";

        //InitCreateInsuranceFlow
        public static readonly string START_INSURANCE_WORKFLOW_FALLBACK_TEXT = "Starting getting an insurance for you. Please, provide a photo of your ID and driver license.";
        public static readonly GPTTextSetting START_INSURANCE_WORKFLOW_SETTINGS = new()
        {
            Stage = "Insurance workflow",
            State = "user started insurance ordering workflow",
            Action = "User started insurance ordering workflow.",
            AnswerReq = "Write about starting getting insurance for user and ask to share a photos of ID and driver license via authorization system",
            FallbackText = START_INSURANCE_WORKFLOW_FALLBACK_TEXT,
        };

        //Authorization Shared
        public static readonly string AUTHORIZATION_DECLINE_BUTTON_TEXT = "I have reconsidered this";
        public static readonly string SHARE_DOCUMENTS_BUTTON_TEXT = "Share via Passport";
        public static readonly string REDIRECT_URL = "https://gogas1.github.io/CarInsuranceBot/redirect.html?{0}";

        private static AuthorizationRequestParameters GetAuthorizationRequestParameters(ITelegramBotClient botClient, BotConfiguration botConfig, string nonce)
        {
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

        public static InlineKeyboardButton[][] GetAuthorizationKeyboard(ITelegramBotClient client, BotConfiguration botConfig, string nonce)
        {
            var authReq = GetAuthorizationRequestParameters(client, botConfig, nonce);
            InlineKeyboardButton[][] keyboard = [
                [InlineKeyboardButton.WithUrl(
                    AnswersData.SHARE_DOCUMENTS_BUTTON_TEXT,
                    string.Format(AnswersData.REDIRECT_URL, authReq.Query))],
                [new InlineKeyboardButton(AUTHORIZATION_DECLINE_BUTTON_TEXT, "decline")]
                ];
            return keyboard;
        }

        //ProcessDocumentsDataAction
        public static readonly string START_PROCESSING_FALLBACK_TEXT = "Your data has been received. We are processing it.";
        public static readonly GPTTextSetting START_PROCESSING_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "application received user documents",
            Action = "User sent the documents using application authorization workflow system",
            AnswerReq = "Write we have received user's data and it is under processing",
            FallbackText = START_PROCESSING_FALLBACK_TEXT,
        };

        public static readonly string NO_DOCUMENTS_PROVIDED_FALLBACK_TEXT = "You have not provided documents. Please try again";
        public static readonly GPTTextSetting NO_DOCUMENTS_PROVIDED_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "application failed to receive user documents",
            Action = "User failed to send documents using application authorization workflow system",
            AnswerReq = "Write we have not received data from the user, so they need to try again",
            FallbackText = NO_DOCUMENTS_PROVIDED_FALLBACK_TEXT,
        };

        public static readonly string NO_CREDENTIALS_FALLLBACK_TEXT = "Credentials from the passport data are missing. Please try again";
        public static readonly GPTTextSetting NO_CREDENTIALS_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "application failed to get credentials from user data",
            Action = "User sent the documents using application authorization workflow system, but application failed to get credentials from the authorization system",
            AnswerReq = "Write we have failed to get credentials data from the user data, so user need to try again",
            FallbackText = NO_CREDENTIALS_FALLLBACK_TEXT,
        };

        public static readonly string NO_NONCE_TEXT = "Authorization data have been expired. Please resubmit your data again";

        public static readonly string NO_DOCUMENTS_DATA_TEXT = "Cannot extract document data, try make photo again";

        public static readonly string NO_DOCUMENT_DATA_FALLBACK_TEXT = "We couldn't extract data from some of the provided documents. Please try again.";
        public static readonly GPTTextSetting NO_DOCUMENT_DATA_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "application failed to get data from the data extraction api",
            Action = "Application has received answer from the data extraction api, but it failed to extract info from some of the documents",
            AnswerReq = "Write we have failed to extract data from some of the user's document, so user need to try again",
            FallbackText = NO_DOCUMENT_DATA_FALLBACK_TEXT,
        };

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
            Last name - {9};
            First Name - {10};
            Birth date - {11};
            Expiry date - {12};
            """;

        //Data confirmation shared
        public static readonly string DATA_CONFIRMATION_BUTTON_TEXT = "Yes, I confirm";
        public static readonly string DATA_DECLINE_BUTTON_TEXT = "No, data is incorrect";
        public static readonly string DATA_CONFIRMATION_BUTTON_DATA = "yes";
        public static readonly string DATA_DECLINE_BUTTON_DATA = "no";

        public static readonly InlineKeyboardButton[] DATA_CONFIRMATION_KEYBOARD = [
                new InlineKeyboardButton(DATA_CONFIRMATION_BUTTON_TEXT, DATA_CONFIRMATION_BUTTON_DATA),
                new InlineKeyboardButton(DATA_DECLINE_BUTTON_TEXT, DATA_DECLINE_BUTTON_DATA)
                ];

        //Price agreement shared
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

        //ProcessDataConfirmationAction
        public static readonly string DATA_DECLINED_FALLBACK_TEXT = "Please resubmit your documents.";
        public static readonly GPTTextSetting DATA_DECLINED_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "documents correctness declined",
            Action = "User got asked about correctness of the extracted documents data and declined it",
            AnswerReq = "Write about resubmitting user's documents",
            FallbackText = DATA_DECLINED_FALLBACK_TEXT,
        };

        public static readonly string DATA_CONFIRMED_FALLBACK_TEXT = "Thanks. Insurance fixed price is 100 US dollars. Are you agreeing with this price and ready to proceed?";
        public static readonly GPTTextSetting DATA_CONFIRMED_SETTINGS = new()
        {
            Stage = "Documents processing",
            State = "documents correctness confirmed",
            Action = "User got asked about correctness of the extracted documents data and confirmed it",
            AnswerReq = "Thank user. Offer insurance 100 US dollars fixed price and ask about agreement with such price",
            FallbackText = DATA_CONFIRMED_FALLBACK_TEXT,
        };
    }
}
