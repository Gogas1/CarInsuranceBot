using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Models.Documents
{
    public class IdDocument
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public List<string> Surnames { get; set; } = new();
        public List<string> Names { get; set; } = new();
        public DateTime BirthDate { get; set; } = DateTime.MinValue;
        public DateTime ExpiryDate { get; set; } = DateTime.MinValue;

        public bool IsValid()
        {
            return 
                !string.IsNullOrEmpty(DocumentNumber) &&
                !string.IsNullOrEmpty(CountryCode) &&
                Surnames.Any() &&
                Names.Any() &&
                BirthDate != DateTime.MinValue &&
                ExpiryDate != DateTime.MinValue;
        }
    }
}
