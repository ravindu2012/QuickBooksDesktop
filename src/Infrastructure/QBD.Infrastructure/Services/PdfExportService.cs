using System;
using System.IO;
using System.Threading.Tasks;
using QBD.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QBD.Infrastructure.Services
{
    public class PdfExportService : IPdfExportService
    {
        public PdfExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task ExportInvoiceToPdfAsync(object invoiceData, string filePath)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
            });

            document.GeneratePdf(filePath);
            return Task.CompletedTask;
        }

        public Task ExportReportToPdfAsync(string reportTitle, object reportData, string filePath)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    page.Header().Text(reportTitle).SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    page.Content().PaddingVertical(1, Unit.Centimetre).Text("Tukaj bo kmalu tabela z dejanskimi podatki poročila...");
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            document.GeneratePdf(filePath);
            return Task.CompletedTask;
        }


        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("INVOICE").FontSize(28).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Date: {DateTime.Now:d}").FontSize(14);
                    column.Item().Text("Invoice #: INV-1001").FontSize(14);
                });

                row.ConstantItem(100).Height(50).Placeholder();
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text("Billed To:").SemiBold();
                column.Item().Text("John Doe\n123 Main Street\nNew York, NY 10001");
                column.Item().PaddingTop(25).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // #
                        columns.RelativeColumn(3);  // Item
                        columns.RelativeColumn();   // Qty
                        columns.RelativeColumn();   // Rate
                        columns.RelativeColumn();   // Amount
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("#").SemiBold();
                        header.Cell().Text("Item").SemiBold();
                        header.Cell().AlignRight().Text("Qty").SemiBold();
                        header.Cell().AlignRight().Text("Rate").SemiBold();
                        header.Cell().AlignRight().Text("Amount").SemiBold();
                    });

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text("1");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text("Consulting Services");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignRight().Text("10");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignRight().Text("$150.00");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignRight().Text("$1500.00");
                });

                column.Item().PaddingTop(10).AlignRight().Text("Total: $1500.00").FontSize(16).SemiBold();
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        }
    }
}