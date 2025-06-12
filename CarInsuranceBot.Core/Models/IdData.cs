using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Models
{
    internal class IdData
    {
        public string Number { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
