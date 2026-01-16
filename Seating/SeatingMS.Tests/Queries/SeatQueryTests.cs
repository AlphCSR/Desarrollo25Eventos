using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SeatingMS.Application.Queries.GetSeatsByEvent;
using SeatingMS.Domain.Entities;
using SeatingMS.Domain.Interfaces;
using Xunit;

namespace SeatingMS.Tests.Queries
{
    public class SeatQueryTests
    {
        private readonly Mock<IEventSeatRepository> _repositoryMock;

        public SeatQueryTests()
        {
            _repositoryMock = new Mock<IEventSeatRepository>();
        }

        [Fact]
        public async Task Handle_GetSeatsByEvent_Success()
        {
            
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var seats = new List<EventSeat>
            {
                new EventSeat(eventId, Guid.NewGuid(), "A", 1),
                new EventSeat(eventId, Guid.NewGuid(), "A", 2)
            };
            
            seats[1].Lock(userId);

            _repositoryMock.Setup(x => x.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seats);

            var handler = new GetSeatsByEventQueryHandler(_repositoryMock.Object);
            var query = new GetSeatsByEventQuery(eventId);

            
            var result = await handler.Handle(query, CancellationToken.None);

            
            result.Should().HaveCount(2);
            result.First().Row.Should().Be("A");
            
            var lockedSeatDto = result.FirstOrDefault(s => s.Row == "A" && s.Number == 2);
            lockedSeatDto.Should().NotBeNull();
            lockedSeatDto!.UserId.Should().Be(userId);
        }
    }
}
