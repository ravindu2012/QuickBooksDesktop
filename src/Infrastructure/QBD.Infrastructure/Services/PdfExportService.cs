using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Customers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QBD.Infrastructure.Services
{
    public class PdfExportService : IPdfExportService
    {
        static PdfExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task ExportInvoiceToPdfAsync(Invoice invoice, string filePath)
        {
            ArgumentNullException.ThrowIfNull(invoice);

            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("INVOICE").FontSize(28).SemiBold().FontColor(Colors.Blue.Darken2);
                                column.Item().Text($"Date: {invoice.Date:d}").FontSize(12);
                                column.Item().Text($"Invoice #: {invoice.InvoiceNumber ?? "N/A"}").FontSize(12);
                            });

                            row.ConstantItem(100).Height(50).Placeholder();
                        });

                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                        {
                            column.Spacing(5);
                            column.Item().Text("Billed To:").SemiBold();
                            
                            var customerName = invoice.Customer?.CustomerName ?? "No Customer Selected";
                            var address = invoice.BillToAddress ?? "No Address Provided";
                            column.Item().Text($"{customerName}\n{address}");

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
                                    header.Cell().BorderBottom(1).Text("#").SemiBold();
                                    header.Cell().BorderBottom(1).Text("Item").SemiBold();
                                    header.Cell().BorderBottom(1).AlignRight().Text("Qty").SemiBold();
                                    header.Cell().BorderBottom(1).AlignRight().Text("Rate").SemiBold();
                                    header.Cell().BorderBottom(1).AlignRight().Text("Amount").SemiBold();
                                });

                                int index = 1;
                                foreach (var line in invoice.Lines ?? new List<InvoiceLine>())
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(index++.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(line.Item?.ItemName ?? line.Description ?? "Item");

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignRight().Text(line.Qty.ToString()); 

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignRight().Text($"{line.Rate:C}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignRight().Text($"{line.Amount:C}");
                                }
                            });

                            column.Item().PaddingTop(10).AlignRight().Text($"Total: {invoice.Total:C}").FontSize(16).SemiBold();
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        public async Task ExportReportToPdfAsync(string reportTitle, object reportData, string filePath)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);

                        page.Header().Text(reportTitle).SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                        page.Content().PaddingVertical(1, Unit.Centimetre).Text("Report data export is currently in development...");
                        
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                })
                .GeneratePdf(filePath);
            });
        }
    }
}