namespace CarInsuranceBot.Core.Models.Documents
{
    public class DriverLicenseDocument
    {
        public string CountryCode { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; } = DateTime.MinValue;
        public DateTime ExpiryDate { get; set; } = DateTime.MinValue;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(CountryCode) &&
                !string.IsNullOrEmpty(Id) &&
                !string.IsNullOrEmpty(Category) &&
                !string.IsNullOrEmpty(LastName) &&
                !string.IsNullOrEmpty(FirstName) &&
                BirthDate != DateTime.MinValue &&
                ExpiryDate != DateTime.MinValue;
        }
    }
}
