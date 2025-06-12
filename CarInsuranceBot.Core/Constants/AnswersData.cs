using CarInsuranceBot.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Core.Constants
{
    internal static class AnswersData
    {
        // DefaultHomeMessage
        public static readonly string DefaultHomeTextFallback = "Proceed with available actions";
        public static readonly string GetInsuranceButtonText = "Get Insurance";
        public static readonly InlineKeyboardButton[] GetInsuranceInlineButton = [new InlineKeyboardButton(GetInsuranceButtonText, "get_insurance")];

        //MessageActionBase
        public static readonly string ResetMessage = "Your bot state has been reset";

        //Authorization Shared
        public static readonly string ShareDocumentButtonText = "Share via Passport";
        public static readonly string RedirectUrl = "https://gogas1.github.io/CarInsuranceBot/redirect.html?{0}";

        public static AuthorizationRequestParameters GetAuthorizationRequestParameters(ITelegramBotClient botClient, BotConfiguration botConfig, string nonce)
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

        //ProcessDocumentsDataAction
        public static readonly string StartProcessingFallbackText = "Your data has been received. We a processing it.";
        public static readonly string NoFileMessageFallbackText = "You have not provided an ID photo";
        public static readonly string NoCredentialsMessageFallbackText = "Credentials from the passport data are missing";
        public static readonly string NoNonceMessageFallbackText = "Authorization data have been expired. Try again";
        public static readonly string NoIdDataFallbackText = "Cannot extract passport data, try make photo again";
        public static readonly string NoDocumentsFallbackText = "We couldn't extract data from the some of the provided documents. Please try again.";
        public static readonly string DocumentsProvidedFallbackText = """
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
        public static readonly string DataConfirmationButtonText = "Yes, I confirm";
        public static readonly string DataDeclineButtonText = "No, data is incorrect";
        public static readonly string DataConfirmationButtonData = "yes";
        public static readonly string DataDeclineButtonData = "no";

        public static readonly InlineKeyboardButton[] DataConfirmationKeyboard = [
                new InlineKeyboardButton(DataConfirmationButtonText, DataConfirmationButtonData),
                new InlineKeyboardButton(DataDeclineButtonText, DataDeclineButtonData)
                ];

        //Price agreement shared
        public static readonly string NoStoredDocumentsText = "Your information is no longer stored. Resubmit it please.";
        public static readonly string InsuranceGrantedText = "Your insurance has been ordered. Keep the provided document as a confirmaion.";
        public static readonly string FirstPriceDeclineText = "We are sorry, the price 100 US dollars is only available price. Do you agree with this price?";
        public static readonly string SecondPriceDeclineText = "";

        public static readonly string PriceConfirmationButtonText = "Yes, I agree";
        public static readonly string PriceDeclineButtonText = "No, I don't agree";
        public static readonly string PriceConfirmationButtonData = "yes";
        public static readonly string PriceDeclineButtonData = "no";

        public static readonly InlineKeyboardButton[] PriceConfirmationKeyboard = [
                new InlineKeyboardButton(PriceConfirmationButtonText, PriceConfirmationButtonData),
                new InlineKeyboardButton(PriceDeclineButtonText, PriceDeclineButtonData)
                ];

        //ProcessDataConfirmationAction
        public static readonly string DataDeclinedFallbackText = "Please resubmit your documents.";
        public static readonly string DataConfirmedFallbackText = "Thanks. Insurance fixed price is 100 US dollars. Are you agreeing with this price and ready to proceed?";
        public static readonly string NoTextFallbackText = "You have not provided a concrete answer. You can confirm or decline corectness of the extracted data in message, but it is better to use provided buttons.";
    }
}
