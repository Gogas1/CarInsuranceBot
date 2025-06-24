namespace CarInsuranceBot.Core.Models.Documents
{
    /// <summary>
    /// Id document data model
    /// </summary>
    public class IdDocument
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; } = DateTime.MinValue;
        public DateTime ExpiryDate { get; set; } = DateTime.MinValue;

        public bool IsValid()
        {
            return
                !string.IsNullOrEmpty(DocumentNumber) &&
                !string.IsNullOrEmpty(CountryCode) &&
                !string.IsNullOrEmpty(Surname) &&
                !string.IsNullOrEmpty(Name) &&
                BirthDate != DateTime.MinValue &&
                ExpiryDate != DateTime.MinValue;
        }

        internal List<DocumentFieldModel<IdDocument>> GetInvalidFieldsHandlers()
        {
            var invalidations = new List<DocumentFieldModel<IdDocument>>();

            if (string.IsNullOrEmpty(DocumentNumber)) invalidations.Add(new("Document number", SetDocumentNumberFromString));
            if (string.IsNullOrEmpty(CountryCode)) invalidations.Add(new("Country code", SetCountryCodeFromString));
            if (string.IsNullOrEmpty(Surname)) invalidations.Add(new("Surname", SetSurnameFromString));
            if (BirthDate == DateTime.MinValue) invalidations.Add(new("Birth date", SetBirthDateFromString));
            if (ExpiryDate == DateTime.MinValue) invalidations.Add(new("Expiry date", SetExpiryDateFromString));

            return invalidations;
        }

        private bool SetDocumentNumberFromString(string? objStr)
        {
            if (string.IsNullOrEmpty(objStr))
            {
                return false;
            }

            DocumentNumber = objStr;
            return true;
        }

        private bool SetCountryCodeFromString(string? objStr)
        {
            if (string.IsNullOrEmpty(objStr))
            {
                return false;
            }

            CountryCode = objStr;
            return true;
        }

        private bool SetSurnameFromString(string? objStr)
        {
            if (string.IsNullOrEmpty(objStr))
            {
                return false;
            }

            Surname = objStr;
            return true;
        }

        private bool SetNameFromString(string? objStr)
        {
            if (string.IsNullOrEmpty(objStr))
            {
                return false;
            }

            Name = objStr;
            return true;
        }

        private bool SetBirthDateFromString(string? objStr)
        {
            if (!DateTime.TryParse(objStr, out var result))
            {
                return false;
            }

            BirthDate = result;
            return true;
        }

        private bool SetExpiryDateFromString(string? objStr)
        {
            if (!DateTime.TryParse(objStr, out var result))
            {
                return false;
            }

            ExpiryDate = result;
            return true;
        }
    }
}
