using System;
using ReportsMS.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ReportsMS.Tests.Domain
{
    public class DashboardMetricTests
    {
        [Fact]
        public void DashboardMetric_Creation_ShouldSetProperties()
        {
            var name = "Revenue";
            var value = 1500.50m;
            var dimensions = "Event:Concert";

            var metric = new DashboardMetric(name, value, dimensions);

            metric.MetricName.Should().Be(name);
            metric.Value.Should().Be(value);
            metric.Dimensions.Should().Be(dimensions);
            metric.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void DashboardMetric_UpdateValue_ShouldUpdateValueAndTimestamp()
        {
            var metric = new DashboardMetric("Test", 100);
            var initialTimestamp = metric.Timestamp;

            // Wait a bit to ensure timestamp changes
            System.Threading.Thread.Sleep(10); 

            metric.UpdateValue(200);

            metric.Value.Should().Be(200);
            metric.Timestamp.Should().BeAfter(initialTimestamp);
        }
    }
}
