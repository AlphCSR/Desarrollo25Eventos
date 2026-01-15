using System;

namespace EventsMS.Domain.ValueObjects
{
    public record DateRange
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        private DateRange(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public static DateRange Create(DateTime startDate, DateTime endDate)
        {
            if (startDate < DateTime.UtcNow.AddMinutes(-5)) 
                throw new ArgumentException("La fecha de inicio no puede ser en el pasado.");

            if (endDate <= startDate)
                throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio.");

            return new DateRange(startDate, endDate);
        }

        public bool Overlaps(DateRange other)
        {
            return StartDate < other.EndDate && other.StartDate < EndDate;
        }
    }
}
