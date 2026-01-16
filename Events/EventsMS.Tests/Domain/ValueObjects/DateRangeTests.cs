using FluentAssertions;
using EventsMS.Domain.ValueObjects;
using Xunit;
using System;

namespace EventsMS.Tests.Domain.ValueObjects
{
    public class DateRangeTests
    {
        [Theory]
        [InlineData(0, 1, true, null)]
        [InlineData(-10, 1, false, "La fecha de inicio no puede ser en el pasado.")]
        [InlineData(1, 0, false, "La fecha de fin debe ser posterior a la fecha de inicio.")]
        [InlineData(2, -1, false, "La fecha de fin debe ser posterior a la fecha de inicio.")]
        public void Create_AllCases(int startHoursOffset, int endHoursOffset, bool shouldSucceed, string expectedErrorMessage)
        {
            var now = DateTime.UtcNow;
            var start = now.AddHours(startHoursOffset);
            var end = now.AddHours(startHoursOffset + endHoursOffset);

            if (shouldSucceed)
            {
                var result = DateRange.Create(start, end);
                result.Should().NotBeNull();
                result.StartDate.Should().Be(start);
                result.EndDate.Should().Be(end);
            }
            else
            {
                Action act = () => DateRange.Create(start, end);
                act.Should().Throw<ArgumentException>()
                   .WithMessage(expectedErrorMessage);
            }
        }

        [Fact]
        public void Overlaps_Tests()
        {
            var now = DateTime.UtcNow.AddDays(1);
            var range1 = DateRange.Create(now, now.AddHours(2));
            
            var range2 = DateRange.Create(now.AddHours(1), now.AddHours(3));
            range1.Overlaps(range2).Should().BeTrue();

            var range3 = DateRange.Create(now.AddHours(3), now.AddHours(4));
            range1.Overlaps(range3).Should().BeFalse();
        }
    }
}
