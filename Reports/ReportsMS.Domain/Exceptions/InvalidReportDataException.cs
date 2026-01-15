using System;

namespace ReportsMS.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }
    }

    public class InvalidReportDataException : DomainException
    {
        public InvalidReportDataException(string message) : base(message)
        {
        }
    }
}
