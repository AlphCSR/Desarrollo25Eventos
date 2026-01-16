using System;
using System.Threading;
using System.Threading.Tasks;
using SurveyMS.Application.Commands.SubmitFeedback;
using SurveyMS.Domain.Entities;
using SurveyMS.Domain.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace SurveyMS.Tests.Handlers
{
    public class SurveyHandlerTests
    {
        private readonly Mock<IFeedbackRepository> _repositoryMock;

        public SurveyHandlerTests()
        {
            _repositoryMock = new Mock<IFeedbackRepository>();
        }

        [Fact]
        public async Task Handle_SubmitFeedback_Tests()
        {
            var handler = new SubmitFeedbackCommandHandler(_repositoryMock.Object);
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var command = new SubmitFeedbackCommand(eventId, userId, bookingId, "User", 5, "Good");

            // --- Case 1: Success ---
            _repositoryMock.Setup(x => x.HasUserRatedBookingAsync(userId, bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            
            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeEmpty();
            _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Feedback>(), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // --- Case 2: Already Rated ---
            _repositoryMock.Setup(x => x.HasUserRatedBookingAsync(userId, bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Ya has enviado un comentario para esta reserva.");
        }
    }
}
