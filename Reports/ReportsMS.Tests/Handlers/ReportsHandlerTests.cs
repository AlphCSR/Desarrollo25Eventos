using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReportsMS.Application.Queries;
using ReportsMS.Domain.Entities;
using ReportsMS.Domain.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace ReportsMS.Tests.Handlers
{
    public class ReportsHandlerTests
    {
        private readonly Mock<IReportsRepository> _repositoryMock;

        public ReportsHandlerTests()
        {
            _repositoryMock = new Mock<IReportsRepository>();
        }

        [Fact]
        public async Task Handle_GetDashboardMetrics_ShouldReturnMetrics()
        {
            var handler = new GetDashboardMetricsQueryHandler(_repositoryMock.Object);
            var metrics = new List<DashboardMetric>
            {
                new DashboardMetric("M1", 10),
                new DashboardMetric("M2", 20)
            };

            _repositoryMock.Setup(x => x.GetAllMetricsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(metrics);

            var result = await handler.Handle(new GetDashboardMetricsQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
            result.Should().Contain(m => m.MetricName == "M1");
        }

        [Fact]
        public async Task Handle_GetSalesReport_ShouldReturnCorrectTotals()
        {
            var handler = new GetSalesReportQueryHandler(_repositoryMock.Object);
            var eventId = Guid.NewGuid();
            var sales = new List<SalesRecord>
            {
                new SalesRecord(eventId, Guid.NewGuid(), Guid.NewGuid(), "u1@test.com", 100.0m, DateTime.UtcNow),
                new SalesRecord(eventId, Guid.NewGuid(), Guid.NewGuid(), "u2@test.com", 50.0m, DateTime.UtcNow)
            };

            _repositoryMock.Setup(x => x.GetSalesByEventAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(sales);

            var result = await handler.Handle(new GetSalesReportQuery(eventId), CancellationToken.None);

            result.EventId.Should().Be(eventId);
            result.TotalBookings.Should().Be(2);
            result.TotalRevenue.Should().Be(150.0m);
        }

        [Fact]
        public async Task Handle_GetEventDetailedReport_ShouldReturnDetails()
        {
            var handler = new GetEventDetailedReportQueryHandler(_repositoryMock.Object);
            var eventId = Guid.NewGuid();
            var sales = new List<SalesRecord>
            {
                new SalesRecord(eventId, Guid.NewGuid(), Guid.NewGuid(), "u1@test.com", 100.0m, DateTime.UtcNow)
            };
            var stats = new EventStats(eventId, 500);

            _repositoryMock.Setup(x => x.GetSalesByEventAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(sales);
            _repositoryMock.Setup(x => x.GetEventStatsAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats);

            var result = await handler.Handle(new GetEventDetailedReportQuery(eventId), CancellationToken.None);

            result.Should().NotBeNull();
            result.TotalCapacity.Should().Be(500);
            result.TotalRevenue.Should().Be(100.0m);
            result.Attendees.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_GetLiveReport_Sales_ShouldReturnDailySales()
        {
            var handler = new GetLiveReportQueryHandler(_repositoryMock.Object);
            var expectedData = new List<object> { new { Date = DateTime.UtcNow, Total = 100m, Count = 1 } };

            _repositoryMock.Setup(x => x.GetDailySalesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedData);

            var result = await handler.Handle(new GetLiveReportQuery { ReportType = "sales" }, CancellationToken.None);

            var resultType = result.GetType();
            resultType.GetProperty("title")?.GetValue(result).Should().Be("Ventas Diarias (Últimos 30 días)");
        }

        [Fact]
        public async Task Handle_GetLiveReport_Summary_ShouldReturnMetrics()
        {
            var handler = new GetLiveReportQueryHandler(_repositoryMock.Object);
            var metrics = new List<DashboardMetric> { new DashboardMetric("M1", 100) };

            _repositoryMock.Setup(x => x.GetAllMetricsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(metrics);

            var result = await handler.Handle(new GetLiveReportQuery { ReportType = "summary" }, CancellationToken.None);

            var resultType = result.GetType();
            resultType.GetProperty("title")?.GetValue(result).Should().Be("Resumen Ejecutivo (Métricas Cacheadas)");
        }
    }
}
