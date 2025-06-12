using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Models.StateFlows
{
    public class CreateInsuranceFlow
    {
        public string IdCacheKey { get; set; } = string.Empty;
        public string DriverLicenseCacheKey { get; set; } = string.Empty;
    }
}
