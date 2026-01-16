using System;
using System.Threading.Tasks;
using BookingMS.Shared.Events;
using EventsMS.Shared.Events;
using ServicesMS.Shared.Events;
using MassTransit;
using Moq;
using NotificationsMS.Application.Interfaces;
using NotificationsMS.Domain.Interfaces;
using NotificationsMS.Infrastructure.Consumers;
using Xunit;

namespace NotificationsMS.Tests.Consumers
{
    public class OtherConsumersTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IEmailTemplateService> _templateServiceMock;
        private readonly Mock<INotifier> _notifierMock;

        public OtherConsumersTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
            _templateServiceMock = new Mock<IEmailTemplateService>();
            _notifierMock = new Mock<INotifier>();
        }

        [Fact]
        public async Task BookingCancelled_ShouldSendEmailAndNotification()
        {
            var consumer = new BookingCancelledConsumer(_notifierMock.Object, _emailServiceMock.Object, _templateServiceMock.Object);
            var bookingId = Guid.NewGuid();
            var contextMock = new Mock<ConsumeContext<BookingCancelledEvent>>();
            var message = new BookingCancelledEvent 
            { 
                BookingId = bookingId,
                Email = "test@test.com",
                Language = "es",
                EventName = "Event",
                UserId = Guid.NewGuid(),
                Reason = "Test"
            };

            contextMock.Setup(x => x.Message).Returns(message);
            _templateServiceMock.Setup(x => x.GetBookingCancelledTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(("Asunto", "Cuerpo"));

            await consumer.Consume(contextMock.Object);

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), "Asunto", "Cuerpo"), Times.Once);
            _notifierMock.Verify(x => x.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PaymentExpiringSoon_ShouldSendEmailAndNotification()
        {
            var consumer = new PaymentExpiringSoonConsumer(_notifierMock.Object, _emailServiceMock.Object);
            var bookingId = Guid.NewGuid();
            var contextMock = new Mock<ConsumeContext<PaymentExpiringSoonEvent>>();
            var message = new PaymentExpiringSoonEvent
            {
                BookingId = bookingId,
                Email = "test@test.com",
                UserId = Guid.NewGuid(),
                MinutesRemaining = 5
            };

            contextMock.Setup(x => x.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.Is<string>(s => s.Contains("expirar")), It.IsAny<string>()), Times.Once);
            _notifierMock.Verify(x => x.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PaymentFailed_ShouldNotifyUser()
        {
            var consumer = new PaymentFailedConsumer(_emailServiceMock.Object, _notifierMock.Object);
            var bookingId = Guid.NewGuid();
            var contextMock = new Mock<ConsumeContext<PaymentFailedEvent>>();
            var message = new PaymentFailedEvent
            {
                BookingId = bookingId,
                Email = "fail@test.com",
                Reason = "Insufficent funds",
                UserId = Guid.NewGuid()
            };

            contextMock.Setup(x => x.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            _emailServiceMock.Verify(x => x.SendEmailAsync("fail@test.com", "Pago Fallido", It.IsAny<string>()), Times.Once);
            _notifierMock.Verify(x => x.SendNotificationAsync("fail@test.com", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ServiceBooked_ShouldSendEmail()
        {
            var consumer = new ServiceBookedConsumer(_emailServiceMock.Object);
            var contextMock = new Mock<ConsumeContext<ServiceBookedEvent>>();
            var message = new ServiceBookedEvent 
            {
                ServiceBookingId = Guid.NewGuid(),
                ServiceDefinitionId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Quantity = 2,
                TotalPrice = 50.0m
            };

            contextMock.Setup(x => x.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.Is<string>(s => s.Contains("Adicional")), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task EventCancelled_ShouldBroadcastNotification()
        {
            var consumer = new EventCancelledConsumer(_notifierMock.Object);
            var contextMock = new Mock<ConsumeContext<EventCancelledEvent>>();
            var message = new EventCancelledEvent { EventId = Guid.NewGuid(), Title = "Cancelled Event", Reason = "Force Majeure" };

            contextMock.Setup(x => x.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            _notifierMock.Verify(x => x.BroadcastSeatUpdateAsync(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task EventStatusChanged_ShouldBroadcastUpdate()
        {
            var consumer = new EventStatusChangedConsumer(_notifierMock.Object);
            var contextMock = new Mock<ConsumeContext<EventStatusChangedEvent>>();
            var message = new EventStatusChangedEvent { EventId = Guid.NewGuid(), NewStatus = EventsMS.Shared.Enums.EventStatus.Published, Timestamp = DateTime.UtcNow };

            contextMock.Setup(x => x.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            _notifierMock.Verify(x => x.BroadcastSeatUpdateAsync(It.IsAny<object>()), Times.Once);
        }

    }
}
