using System;

namespace InvoicingMS.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }
    }

    public class InvoicingException : DomainException
    {
        public InvoicingException(string message) : base(message)
        {
        }
    }
}
