using System;
using System.Threading.Tasks;
using BookingMS.Shared.Events;
using MassTransit;
using Moq;
using NotificationsMS.Application.Interfaces;
using NotificationsMS.Domain.Interfaces;
using NotificationsMS.Infrastructure.Consumers;
using Xunit;

namespace NotificationsMS.Tests.Consumers
{
    public class BookingConfirmedConsumerTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IEmailTemplateService> _templateServiceMock;
        private readonly Mock<INotifier> _notifierMock;
        private readonly BookingConfirmedConsumer _consumer;

        public BookingConfirmedConsumerTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
            _templateServiceMock = new Mock<IEmailTemplateService>();
            _notifierMock = new Mock<INotifier>();
            _consumer = new BookingConfirmedConsumer(_emailServiceMock.Object, _templateServiceMock.Object, _notifierMock.Object);
        }

        [Fact]
        public async Task Consume_ShouldSendEmailAndNotification()
        {
            var bookingId = Guid.NewGuid();
            var email = "user@example.com";
            var contextMock = new Mock<ConsumeContext<BookingConfirmedEvent>>();
            var message = new BookingConfirmedEvent
            {
                BookingId = bookingId,
                Email = email,
                EventName = "Grand Concert",
                Language = "es",
                UserId = Guid.NewGuid()
            };
            contextMock.Setup(x => x.Message).Returns(message);

            _templateServiceMock.Setup(x => x.GetBookingConfirmedTemplate("es", bookingId.ToString(), "Grand Concert"))
                .Returns(("Confirmación", "Cuerpo del email"));

            await _consumer.Consume(contextMock.Object);

            _emailServiceMock.Verify(x => x.SendEmailAsync(email, "Confirmación", "Cuerpo del email"), Times.Once);
            _notifierMock.Verify(x => x.SendNotificationAsync(email, It.Is<string>(s => s.Contains(bookingId.ToString().Substring(0, 8)))), Times.Once);
        }
    }
}
