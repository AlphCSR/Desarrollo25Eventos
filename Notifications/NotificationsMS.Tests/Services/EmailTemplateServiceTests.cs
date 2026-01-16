using System;
using NotificationsMS.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace NotificationsMS.Tests.Services
{
    public class EmailTemplateServiceTests
    {
        private readonly EmailTemplateService _service;

        public EmailTemplateServiceTests()
        {
            _service = new EmailTemplateService();
        }

        [Theory]
        [InlineData("es", "Confirmación de Reserva")]
        [InlineData("en", "Booking Confirmation")]
        [InlineData("fr", "Confirmación de Reserva")]
        public void GetBookingConfirmedTemplate_ShouldReturnCorrectSubject(string lang, string expectedSubjectStart)
        {
            var (subject, body) = _service.GetBookingConfirmedTemplate(lang, "123", "Concert");
            
            subject.Should().StartWith(expectedSubjectStart);
            body.Should().Contain("123");
            body.Should().Contain("Concert");
        }

        [Theory]
        [InlineData("es", "Reserva Cancelada")]
        [InlineData("en", "Booking Cancelled")]
        public void GetBookingCancelledTemplate_ShouldReturnCorrectSubject(string lang, string expectedSubjectStart)
        {
            var (subject, body) = _service.GetBookingCancelledTemplate(lang, "123", "Concert");
            
            subject.Should().StartWith(expectedSubjectStart);
        }

        [Theory]
        [InlineData("es", "Evento Cancelado")]
        [InlineData("en", "Event Cancelled")]
        public void GetEventCancelledTemplate_ShouldReturnCorrectSubject(string lang, string expectedSubjectStart)
        {
            var (subject, body) = _service.GetEventCancelledTemplate(lang, "Concert");
            
            subject.Should().StartWith(expectedSubjectStart);
        }
    }
}
