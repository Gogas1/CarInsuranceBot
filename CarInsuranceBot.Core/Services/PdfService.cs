using CarInsuranceBot.Core.Constants;
using CarInsuranceBot.Pdf;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Services
{
    internal class PdfService
    {
        public void GenerateInsurancePdf(InsuranceDocumentModel documentData, Stream outStream)
        {
            var document = new InsuranceDocument(documentData);
            document.GeneratePdf(outStream);
        }
    }
}
