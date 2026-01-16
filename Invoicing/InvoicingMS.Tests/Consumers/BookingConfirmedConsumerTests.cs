using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingMS.Shared.Events;
using BookingMS.Shared.Dtos.Response;
using MassTransit;
using Moq;
using InvoicingMS.Infrastructure.Consumers;
using InvoicingMS.Infrastructure.Services;
using InvoicingMS.Shared.Dtos;
using Xunit;

namespace InvoicingMS.Tests.Consumers
{
    public class BookingConfirmedConsumerTests
    {
        private readonly Mock<IPdfGenerator> _pdfGeneratorMock;

        public BookingConfirmedConsumerTests()
        {
            _pdfGeneratorMock = new Mock<IPdfGenerator>();
        }

        [Fact]
        public async Task Consume_ShouldCallPdfGenerator()
        {
            // Arrange
            var consumer = new BookingConfirmedConsumer(_pdfGeneratorMock.Object);
            var contextMock = new Mock<ConsumeContext<BookingConfirmedEvent>>();
            
            var bookingId = Guid.NewGuid();
            var message = new BookingConfirmedEvent
            {
                BookingId = bookingId,
                Email = "test@example.com",
                TotalAmount = 100,
                DiscountAmount = 0,
                Language = "es",
                Items = new List<InvoiceItemDto> 
                { 
                    new InvoiceItemDto("Seat A1", 100, 1, 100) 
                }
            };
            contextMock.Setup(x => x.Message).Returns(message);

            _pdfGeneratorMock.Setup(x => x.GenerateInvoicePdf(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<decimal>(), 
                It.IsAny<decimal>(), 
                It.IsAny<List<InvoiceItemInvoicingDto>>(), 
                It.IsAny<decimal>(), 
                It.IsAny<string>()
            )).Returns(new byte[] { 1, 2, 3 });

            // Act & Assert
            // Note: This might fail in CI if /app/invoices is not accessible, 
            // but in local environments it usually works or throws a clear exception.
            // We want to verify the interaction.
            try 
            {
                await consumer.Consume(contextMock.Object);
            }
            catch (System.IO.DirectoryNotFoundException) 
            {
                // In some environments, /app/invoices won't exist. 
                // We mainly care about the PDF generator call here.
            }
            catch (System.UnauthorizedAccessException)
            {
                // Same for permissions
            }
            catch (System.IO.IOException)
            {
                // General IO
            }

            _pdfGeneratorMock.Verify(x => x.GenerateInvoicePdf(
                It.Is<string>(s => s.Contains(bookingId.ToString().Substring(0, 8))),
                It.IsAny<string>(),
                message.Email,
                message.TotalAmount,
                It.IsAny<decimal>(),
                It.Is<List<InvoiceItemInvoicingDto>>(l => l.Count == 1),
                message.DiscountAmount,
                message.Language
            ), Times.Once);
        }
    }
}
