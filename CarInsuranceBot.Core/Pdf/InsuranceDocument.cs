using CarInsuranceBot.Core.Constants;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarInsuranceBot.Pdf
{
    /// <summary>
    /// Insurance PDF document generation class
    /// </summary>
    internal class InsuranceDocument : IDocument
    {
        public InsuranceDocumentModel Model { get; }

        public InsuranceDocument(InsuranceDocumentModel model)
        {
            Model = model;
        }

        public void Compose(IDocumentContainer container)
        {
            //Create page
            container
                .Page(page =>
                {
                    //With default text font Times New Roman
                    page.DefaultTextStyle(style => style.FontFamily("Times New Roman"));

                    //25mm fields
                    page.Margin(25, Unit.Millimetre);

                    //Compose header
                    page.Header().Element(ComposeHeader);

                    //Compose body
                    page.Content().Element(ComposeContent);

                    //Compose footer
                    page.Footer().Element(ComposeFooter);

                    //Setup page color
                    page.PageColor(Colors.White);
                });
        }

        private void ComposeFooter(IContainer container)
        {
            //In the footer container
            container.Row(row =>
            {
                //Create first 2/5 width item
                row.RelativeItem(2).AlignLeft().Column(column =>
                {
                    //Add representative name
                    column.Item().Text(Model.AuthorizedRepresentative);
                    //Add underline
                    column.Item().PaddingTop(-14).Text("_____________________________");
                    //Add caption
                    column.Item().AlignCenter().PaddingTop(1.15f).Text("Authorized Representative");
                });
                //Create second 1/5 width free space item
                row.RelativeItem(1);
                //Create third 2/5 width item
                row.RelativeItem(2).AlignLeft().Column(column =>
                {
                    //Add date
                    column.Item().AlignCenter().Text(Model.SignDate.ToString("yyyy.MM.dd"));
                    //Add underline
                    column.Item().PaddingTop(-14).Text("_____________________________");
                    //Add caption
                    column.Item().AlignCenter().PaddingTop(1.15f).Text("Date");
                });
            });
        }

        private void ComposeHeader(IContainer container)
        {
            //In the header container
            container.Row(row =>
            {
                //Create item
                row.RelativeItem().Column(column =>
                {
                    //Add 1.15mm text items spacing
                    column.Spacing(1.15f, Unit.Millimetre);
                    //Add header text
                    column.Item()
                        .Text("DUMMY")
                        .FontSize(14).Bold();

                    column.Item()
                        .Text("Insurance Policy")
                        .FontSize(14);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            //In the body container
            container
                .DefaultTextStyle(style => style.FontSize(12))
                .PaddingVertical(18)
                .Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        //Compose table
                        column.Item().Element(ComposeTable);
                        
                        //Add coverage details title
                        column.Item().PaddingTop(18f).Text("Coverage Details").FontSize(14);

                        //Compose coverage details items
                        column.Item().PaddingTop(18f).Element(ComposeCoverageDetails);
                    });
                });
        }

        private void ComposeTable(IContainer container)
        {
            //Init table content dictionary
            Dictionary<string, string> tableContent = new Dictionary<string, string>()
            {
                { InsuranceDocumentData.PolicyNumberText, Model.PolicyNumber },
                { InsuranceDocumentData.InsuredNameText, Model.InsuredName },
                { InsuranceDocumentData.EffectiveDateText, Model.EffectiveDate.ToString("yyyy.MM.dd") },
                { InsuranceDocumentData.ExpirationDateText, Model.ExpirationDate.ToString("yyyy.MM.dd") },
                { InsuranceDocumentData.PremiumAmountText, $"{Model.PremiumAmount.ToString()}$" },
            };

            container.Table(table =>
            {
                //Create two columns
                table.ColumnsDefinition(columns =>
                {
                    //Fixed 4cm first column
                    columns.ConstantColumn(4, Unit.Centimetre);
                    //Relative second column
                    columns.RelativeColumn();
                });

                //Populate cells from the data
                foreach (var item in tableContent)
                {
                    table.Cell().Element(CellStyle).Text(item.Key);
                    table.Cell().Element(CellStyle).Text(item.Value);
                }

                //Cell style function
                static IContainer CellStyle(IContainer container)
                {
                    return container.Border(0.5f).BorderColor(Colors.Black).Padding(0.176f, Unit.Centimetre);
                }
            });
        }

        private void ComposeCoverageDetails(IContainer container)
        {
            container.Column(column =>
            {
                //Init coverage details text. Main item text + string array to format with values
                List<(string, string[])> coverageDetailsContent = new List<(string, string[])>()
                {
                    (InsuranceDocumentData.BodilyInjuryLiabilityText, [Model.BodilyInjuryLimitPerPerson.ToString(), Model.BodilyInjuryLimitPerAccident.ToString()]),
                    (InsuranceDocumentData.PropertyDamageLiabilityText, [Model.PropertyDamageLimit.ToString()]),
                    (InsuranceDocumentData.CollisionCoverageText, [Model.CollisionDeductible.ToString()]),

                    (InsuranceDocumentData.ComprehensiveCoverageText, [Model.ComprehensiveDeductible.ToString()]),
                    (InsuranceDocumentData.PersonalInjuryProtectionText, [Model.PersonalInjuryProtectionLimit.ToString()]),
                    (InsuranceDocumentData.UninsuredUnderinsuredMotoristText, [Model.UMBILimit.ToString(), Model.UMBICombinedLimit.ToString(), Model.UMPDLimit.ToString()]),

                    (InsuranceDocumentData.MedicalPaymentsText, [Model.MedPayLimit.ToString()]),
                    (InsuranceDocumentData.RentalReimbursementText, [Model.RentalRatePerDay.ToString(), Model.RentalMaxDays.ToString()]),
                    (InsuranceDocumentData.RoadsideAssistanceText, [Model.RoadsideLimit.ToString()]),
                };

                //Items spacing
                column.Spacing(1.15f, Unit.Millimetre);

                //Index for the numbered list
                int index = 1;

                //Populate list
                foreach (var item in coverageDetailsContent)
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(0.64f, Unit.Centimetre).Text($"{index}.");
                        row.RelativeItem().Text(string.Format(item.Item1, item.Item2)).Justify();
                    });
                    index++;
                }
            });
        }
    }
}
