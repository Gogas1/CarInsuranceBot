namespace CarInsuranceBot.Pdf
{
    /// <summary>
    /// Insurance PDF documents model
    /// </summary>
    internal class InsuranceDocumentModel
    {
        public string PolicyNumber { get; set; } = string.Empty;
        public string InsuredName { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal BodilyInjuryLimitPerPerson { get; set; }
        public decimal BodilyInjuryLimitPerAccident { get; set; }
        public decimal PropertyDamageLimit { get; set; }
        public decimal CollisionDeductible { get; set; }
        public decimal ComprehensiveDeductible { get; set; }
        public decimal PersonalInjuryProtectionLimit { get; set; }
        public decimal UMBILimit { get; set; }
        public decimal UMBICombinedLimit { get; set; }
        public decimal UMPDLimit { get; set; }
        public decimal MedPayLimit { get; set; }
        public decimal RentalRatePerDay { get; set; }
        public int RentalMaxDays { get; set; }
        public decimal RoadsideLimit { get; set; }

        public string AuthorizedRepresentative { get; set; } = string.Empty;
        public DateTime SignDate { get; set; }
    }
}
