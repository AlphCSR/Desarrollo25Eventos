using FluentAssertions;
using Xunit;
using EventsMS.Application.DTOs;
using EventsMS.Shared.Events;

namespace EventsMS.Tests.DTOs
{
    public class DtoTests
    {
        [Fact]
        public void CreateSectionDto_ShouldSetProperties()
        {
            var dto = new CreateSectionDto("Name", 100, 50, true);
            dto.Name.Should().Be("Name");
            dto.Price.Should().Be(100);
            dto.Capacity.Should().Be(50);
            dto.IsNumbered.Should().BeTrue();
        }

        [Fact]
        public void SectionDto_ShouldSetProperties()
        {
            var dto = new SectionDto(System.Guid.NewGuid(), "Name", 100, 50, true);
            dto.Name.Should().Be("Name");
            dto.Price.Should().Be(100);
            dto.Capacity.Should().Be(50);
            dto.IsNumbered.Should().BeTrue();
        }
    }
}
