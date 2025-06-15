using CarInsuranceBot.Pdf;
using QuestPDF.Fluent;

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
