using CarInsuranceBot.Pdf;
using QuestPDF.Fluent;

namespace CarInsuranceBot.Core.Services
{
    /// <summary>
    /// Service to work with PDF files
    /// </summary>
    internal class PdfService
    {
        /// <summary>
        /// Creates <see cref="InsuranceDocument"/> PDF document and writes into stream
        /// </summary>
        /// <param name="documentData">Document data model</param>
        /// <param name="outStream">Output stream</param>
        public void GenerateInsurancePdf(InsuranceDocumentModel documentData, Stream outStream)
        {
            var document = new InsuranceDocument(documentData);
            document.GeneratePdf(outStream);
        }
    }
}
