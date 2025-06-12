using CarInsuranceBot.Core.Constants;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CarInsuranceBot.Pdf
{
    internal class InsuranceDocument : IDocument
    {
        public InsuranceDocumentModel Model { get; }

        public InsuranceDocument(InsuranceDocumentModel model)
        {
            Model = model;
        }

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.DefaultTextStyle(style => style.FontFamily("Times New Roman"));

                    page.Margin(25, Unit.Millimetre);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);

                    page.Footer().Element(ComposeFooter);

                    page.PageColor(Colors.White);
                });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem(2).AlignLeft().Column(column =>
                {
                    column.Item().Text(Model.AuthorizedRepresentative);
                    column.Item().PaddingTop(-14).Text("_____________________________");
                    column.Item().AlignCenter().PaddingTop(1.15f).Text("Authorized Representative");
                });
                row.RelativeItem(1);
                row.RelativeItem(2).AlignLeft().Column(column =>
                {
                    column.Item().AlignCenter().Text(Model.SignDate.ToString("yyyy.MM.dd"));
                    column.Item().PaddingTop(-14).Text("_____________________________");
                    column.Item().AlignCenter().PaddingTop(1.15f).Text("Date");
                });
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Spacing(1.15f, Unit.Millimetre);
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
            container
                .DefaultTextStyle(style => style.FontSize(12))
                .PaddingVertical(18)
                .Row(row =>
                {
                    row.RelativeItem().Column(column => {

                        column.Item().Element(ComposeTable);
                        
                        column.Item().PaddingTop(18f).Text("Coverage Details").FontSize(14);

                        column.Item().PaddingTop(18f).Element(ComposeCoverageDetails);
                    });
                });
        }

        private void ComposeTable(IContainer container)
        {
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
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(4, Unit.Centimetre);
                    columns.RelativeColumn();
                });

                foreach (var item in tableContent)
                {
                    table.Cell().Element(CellStyle).Text(item.Key);
                    table.Cell().Element(CellStyle).Text(item.Value);
                }

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

                column.Spacing(1.15f, Unit.Millimetre);

                int index = 1;
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
