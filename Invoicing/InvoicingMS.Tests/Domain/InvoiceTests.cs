using System;
using InvoicingMS.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace InvoicingMS.Tests.Domain
{
    public class InvoiceTests
    {
        [Fact]
        public void Invoice_Creation_ShouldCalculateTaxesAndSetNumber()
        {
            var bookingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var total = 100m;
            var pdf = "path/to/pdf";
            var email = "test@example.com";

            var invoice = new Invoice(bookingId, userId, email, total, pdf);

            invoice.TotalAmount.Amount.Should().Be(total);
            invoice.TaxAmount.Amount.Should().Be(total * 0.15m);
            invoice.InvoiceNumber.Should().StartWith("INV-");
            invoice.UserEmail.Value.Should().Be(email);
            invoice.PdfFilePath.Should().Be(pdf);
        }
    }
}
