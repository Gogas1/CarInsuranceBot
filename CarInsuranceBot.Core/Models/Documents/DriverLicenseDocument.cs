namespace CarInsuranceBot.Core.Models.Documents
{
    /// <summary>
    /// Driver license data model
    /// </summary>
    public class DriverLicenseDocument
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.MinValue;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(RegistrationNumber) && RegistrationDate != DateTime.MinValue;
        }

        internal List<DocumentFieldModel<DriverLicenseDocument>> GetInvalidFieldsHandlers()
        {
            var invalidations = new List<DocumentFieldModel<DriverLicenseDocument>>();

            if (string.IsNullOrEmpty(RegistrationNumber)) invalidations.Add(new("Registration number", SetRegistrationNumberFromString));
            if (RegistrationDate == DateTime.MinValue) invalidations.Add(new("Registration date", SetRegistrationDateFromString));

            return invalidations;
        }

        private bool SetRegistrationNumberFromString(string? objStr)
        {
            if(string.IsNullOrEmpty(objStr))
            {
                return false;
            }

            RegistrationNumber = objStr;
            return true;
        }

        private bool SetRegistrationDateFromString(string? objStr)
        {
            if(!DateTime.TryParse(objStr, out var result))
            {
                return false;
            }

            RegistrationDate = result;
            return true;
        }
    }
}
