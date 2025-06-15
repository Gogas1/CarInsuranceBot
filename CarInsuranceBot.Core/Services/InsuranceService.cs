using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Core.Models.Documents;
using CarInsuranceBot.Pdf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Services
{
    internal class InsuranceService
    {
        private readonly DocumentsService _documentsService;

        public InsuranceService(DocumentsService documentsService)
        {
            _documentsService = documentsService;
        }

        public async Task<InsuranceDocumentModel?> CreateInsuranceForUser(long telegramUserId, CancellationToken cancellationToken)
        {
            var data = await _documentsService.GetDataForUserAsync(telegramUserId, cancellationToken);
            if (data == null)
            {
                return null;
            }

            decimal premium = 100m;
            var documentModel = new InsuranceDocumentModel
            {
                PolicyNumber = "PN12345",
                InsuredName = $"{data.idDocument.Names.FirstOrDefault(string.Empty)} {data.idDocument.Surnames.FirstOrDefault(string.Empty)}",
                EffectiveDate = DateTime.Today,
                ExpirationDate = DateTime.Today,
                PremiumAmount = premium,
                BodilyInjuryLimitPerPerson = 500m * premium,
                BodilyInjuryLimitPerAccident = 1000m * premium,
                PropertyDamageLimit = 250m * premium,
                CollisionDeductible = 5m * premium,
                ComprehensiveDeductible = 2.5m * premium,
                PersonalInjuryProtectionLimit = 100m * premium,
                UMBILimit = 300m * premium,
                UMBICombinedLimit = 600m * premium,
                UMPDLimit = 250m * premium,
                MedPayLimit = 50m * premium,
                RentalRatePerDay = 0.35m * premium,
                RentalMaxDays = 30,
                RoadsideLimit = 5m * premium,

                AuthorizedRepresentative = "Jane Smith",
                SignDate = DateTime.Today
            };

            await _documentsService.DeleteUserDataAsync(telegramUserId, cancellationToken);

            return documentModel;
        }
    }
}
