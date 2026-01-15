using System;
using InvoicingMS.Domain.ValueObjects;

namespace InvoicingMS.Domain.Entities
{
    public class Invoice
    {
        public Guid Id { get; private set; }
        public Guid BookingId { get; private set; }
        public Guid UserId { get; private set; }
        public string InvoiceNumber { get; private set; }
        public Money TotalAmount { get; private set; }
        public Money TaxAmount { get; private set; }
        public DateTime IssueDate { get; private set; }
        public string PdfFilePath { get; private set; }
        public Email UserEmail { get; private set; }

        protected Invoice() 
        { 
            InvoiceNumber = null!;
            TotalAmount = null!;
            TaxAmount = null!;
            PdfFilePath = null!;
            UserEmail = null!;
        }

        public Invoice(Guid bookingId, Guid userId, string userEmail, decimal totalAmount, string pdfFilePath)
        {
            Id = Guid.NewGuid();
            BookingId = bookingId;
            UserId = userId;
            UserEmail = Email.Create(userEmail);
            TotalAmount = (Money)totalAmount;
            TaxAmount = (Money)(totalAmount * 0.15m); 
            IssueDate = DateTime.UtcNow;
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Id.ToString().Substring(0, 8).ToUpper()}";
            PdfFilePath = pdfFilePath;
        }
    }
}
