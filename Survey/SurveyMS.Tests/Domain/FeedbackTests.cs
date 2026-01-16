using System;
using SurveyMS.Domain.Entities;
using SurveyMS.Domain.ValueObjects;
using SurveyMS.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace SurveyMS.Tests.Domain
{
    public class FeedbackTests
    {
        [Fact]
        public void Feedback_Creation_ShouldSetProperties()
        {
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var userName = "User Test";
            var rating = 5;
            var comment = "Excellent event!";

            var feedback = new Feedback(eventId, userId, bookingId, userName, rating, comment);

            feedback.EventId.Should().Be(eventId);
            feedback.UserId.Should().Be(userId);
            feedback.BookingId.Should().Be(bookingId);
            feedback.UserName.Should().Be(userName);
            feedback.Rating.Value.Should().Be(rating);
            feedback.Comment.Should().Be(comment);
            feedback.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void Rating_Creation_ShouldThrowOnInvalidValue(int invalidValue)
        {
            Action act = () => Rating.Create(invalidValue);
            act.Should().Throw<InvalidSurveyDataException>()
               .WithMessage("La calificación debe estar entre 1 y 5.");
        }
    }
}
