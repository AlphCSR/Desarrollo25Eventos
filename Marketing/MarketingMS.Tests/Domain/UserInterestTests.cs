using System;
using MarketingMS.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace MarketingMS.Tests.Domain
{
    public class UserInterestTests
    {
        [Fact]
        public void UserInterest_Creation_ShouldSetInitialValues()
        {
            var userId = Guid.NewGuid();
            var category = "Conciertos";

            var interest = new UserInterest(userId, category);

            interest.UserId.Should().Be(userId);
            interest.Category.Should().Be(category);
            interest.Score.Should().Be(1);
            interest.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void IncrementScore_ShouldIncreaseScore()
        {
            var interest = new UserInterest(Guid.NewGuid(), "Teatro");
            interest.IncrementScore();

            interest.Score.Should().Be(2);
        }

        [Fact]
        public void SetScore_ShouldUpdateScore()
        {
            var interest = new UserInterest(Guid.NewGuid(), "Cine");
            interest.SetScore(10);

            interest.Score.Should().Be(10);
        }
    }
}
