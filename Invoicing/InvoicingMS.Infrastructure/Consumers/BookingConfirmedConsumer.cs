
using MassTransit;
using BookingMS.Shared.Events;
using InvoicingMS.Domain.Entities;
using InvoicingMS.Infrastructure.Services;
using InvoicingMS.Shared.Dtos;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InvoicingMS.Infrastructure.Consumers
{
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
    {
        private readonly IPdfGenerator _pdfGenerator;
        private readonly string _storagePath = "/app/invoices"; 

        public BookingConfirmedConsumer(IPdfGenerator pdfGenerator)
        {
            _pdfGenerator = pdfGenerator;
            if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
        }

        public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
        {
            var evt = context.Message;

            var pdfFileName = $"Invoice_{evt.BookingId}.pdf";
            var filePath = Path.Combine(_storagePath, pdfFileName);
            
            var items = evt.Items.Select(i => new InvoiceItemInvoicingDto(
                i.Description,
                i.UnitPrice,
                i.Quantity,
                i.Total
            )).ToList();

            var pdfData = _pdfGenerator.GenerateInvoicePdf(
                $"INV-{evt.BookingId.ToString().Substring(0,8)}",
                System.DateTime.UtcNow.ToString("dd/MM/yyyy"),
                evt.Email,
                evt.TotalAmount,
                evt.TotalAmount * 0.15m,
                items,
                evt.DiscountAmount,
                evt.Language
            );

            await File.WriteAllBytesAsync(filePath, pdfData);

            System.Console.WriteLine($"[InvoicingMS] Factura generada para Booking {evt.BookingId} en {filePath}");
        }
    }
}
