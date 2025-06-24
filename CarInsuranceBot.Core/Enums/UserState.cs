namespace CarInsuranceBot.Core.Enums
{
    /// <summary>
    /// User states enumeration
    /// </summary>
    public enum UserState
    {
        None,
        Home,

        DocumentsAwait,
        DocumentsDataConfirmationAwait,

        PriceConfirmationAwait,
        PriceSecondConfirmationAwait,

        PassportAwait,
        LicenseAwait,

        PassportDataConfirmationAwait,
        LicenseDataConfirmationAwait,

        PassportDataCorrectionAwait,
        LicenseDataCorrectionAwait
    }
}
