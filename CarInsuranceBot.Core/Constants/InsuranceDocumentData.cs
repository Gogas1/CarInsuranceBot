namespace CarInsuranceBot.Core.Constants
{
    /// <summary>
    /// PDF file data
    /// </summary>
    internal static class InsuranceDocumentData
    {
        public static string PolicyNumberText = "Policy Number:";
        public static string InsuredNameText = "Insured Full Name:";
        public static string EffectiveDateText = "Effective Date:";
        public static string ExpirationDateText = "Expiration Date:";
        public static string PremiumAmountText = "Premium Amount:";

        public static string BodilyInjuryLiabilityText = "Bodily Injury Liability – coverage up to ${0} per person and ${1} per accident for bodily injury to others when you’re at fault, including legal defence costs.";
        public static string PropertyDamageLiabilityText = "Property Damage Liability – coverage up to ${0} per accident for damage you cause to someone else’s property.";
        public static string CollisionCoverageText = "Collision Coverage – reimbursement up to the actual cash value of your vehicle, less a deductible of ${0}, for damage resulting from impact with another vehicle or object.";
        public static string ComprehensiveCoverageText = "Comprehensive Coverage – reimbursement up to the actual cash value of your vehicle, less a deductible of ${0}, for non-collision perils (theft, vandalism, fire, vandalism, weather events).";
        public static string PersonalInjuryProtectionText = "Personal Injury Protection (PIP) – coverage up to ${0} per person for medical expenses, lost wages, and related costs, regardless of fault.";
        public static string UninsuredUnderinsuredMotoristText = "Uninsured/Underinsured Motorist – coverage up to ${0} per person and ${1} per accident for bodily injury and ${2} for property damage when the at-fault driver lacks adequate insurance.";
        public static string MedicalPaymentsText = "Medical Payments – coverage up to ${0} per person for reasonable medical expenses resulting from an accident, regardless of fault.";
        public static string RentalReimbursementText = "Rental Reimbursement – coverage up to ${0} per day (maximum {1} days) for a rental vehicle while your car is being repaired due to a covered claim.";
        public static string RoadsideAssistanceText = "Roadside Assistance – reimbursement up to ${0} per incident for services such as towing, battery jump-start, fuel delivery, and lock-out assistance.";
    }
}
