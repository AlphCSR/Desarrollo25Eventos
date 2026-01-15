using System;

namespace MarketingMS.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }
    }

    public class InvalidMarketingDataException : DomainException
    {
        public InvalidMarketingDataException(string message) : base(message)
        {
        }
    }
}
