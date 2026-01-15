
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Collections.Generic;
using InvoicingMS.Shared.Dtos;

namespace InvoicingMS.Infrastructure.Services
{
    public interface IPdfGenerator
    {
        byte[] GenerateInvoicePdf(string invoiceNumber, string date, string customerEmail, decimal total, decimal tax, List<InvoiceItemInvoicingDto> items, decimal discount = 0, string language = "es");
    }

    public class QuestPdfGenerator : IPdfGenerator
    {
        public QuestPdfGenerator()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateInvoicePdf(string invoiceNumber, string date, string customerEmail, decimal total, decimal tax, List<InvoiceItemInvoicingDto> items, decimal discount = 0, string language = "es")
        {
            var isEn = language?.ToLower() == "en";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("TICKET APP").FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text(isEn ? "Event technology solutions" : "Soluciones tecnológicas para eventos").FontSize(9).Italic();
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"{(isEn ? "INVOICE" : "FACTURA")}: {invoiceNumber}").FontSize(14).SemiBold();
                            col.Item().Text($"{(isEn ? "Date" : "Fecha")}: {date}");
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Spacing(20);

                        x.Item().Row(row => {
                            row.RelativeItem().Column(c => {
                                c.Item().Text(isEn ? "CUSTOMER" : "CLIENTE").SemiBold();
                                c.Item().Text(customerEmail);
                            });
                        });

                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text(isEn ? "Description" : "Descripción");
                                header.Cell().Element(CellStyle).AlignRight().Text(isEn ? "Unit" : "Unitario");
                                header.Cell().Element(CellStyle).AlignRight().Text(isEn ? "Qty" : "Cant.");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            int i = 1;
                            foreach (var item in items)
                            {
                                table.Cell().Element(CellValue).Text(i++.ToString());
                                table.Cell().Element(CellValue).Text(item.Description);
                                table.Cell().Element(CellValue).AlignRight().Text($"${item.UnitPrice:N2}");
                                table.Cell().Element(CellValue).AlignRight().Text(item.Quantity.ToString());
                                table.Cell().Element(CellValue).AlignRight().Text($"${(item.UnitPrice * item.Quantity):N2}");
                            }

                            static IContainer CellValue(IContainer container)
                            {
                                return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        x.Item().AlignRight().Column(column =>
                        {
                            column.Spacing(5);
                            var subtotal = total - tax + discount;
                            column.Item().Text($"Subtotal: ${subtotal:N2}");
                            
                            if (discount > 0)
                            {
                                column.Item().Text($"{(isEn ? "Discount" : "Descuento")}: -${discount:N2}").FontColor(Colors.Red.Medium);
                            }

                            column.Item().Text($"{(isEn ? "VAT" : "IVA")} (15%): ${tax:N2}");
                            column.Item().Text($"TOTAL: ${total:N2}").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
                        });
                    });

                    page.Footer().AlignCenter().Column(col => {
                         col.Item().Text(isEn ? "Thank you for choosing TicketApp" : "Gracias por confiar en TicketApp").FontSize(12).SemiBold();
                         col.Item().Text(x =>
                        {
                            x.Span(isEn ? "Page " : "Página ");
                            x.CurrentPageNumber();
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
