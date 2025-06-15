namespace CarInsuranceBot.Core.Enums
{
    public enum UserState
    {
        None,
        Home,

        DocumentsAwait,
        DocumentsDataConfirmationAwait,

        PriceConfirmationAwait,
        PriceSecondConfirmationAwait,

        TestUserState
    }
}
