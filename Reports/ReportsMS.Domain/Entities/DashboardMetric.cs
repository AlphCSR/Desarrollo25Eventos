using System;

namespace ReportsMS.Domain.Entities
{
    public class DashboardMetric
    {
        public Guid Id { get; private set; }
        public string MetricName { get; private set; }
        public decimal Value { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Dimensions { get; private set; }

        public DashboardMetric(string metricName, decimal value, string dimensions = "")
        {
            Id = Guid.NewGuid();
            MetricName = metricName;
            Value = value;
            Timestamp = DateTime.UtcNow;
            Dimensions = dimensions;
        }

        public void UpdateValue(decimal newValue)
        {
            Value = newValue;
            Timestamp = DateTime.UtcNow;
        }
    }
}
