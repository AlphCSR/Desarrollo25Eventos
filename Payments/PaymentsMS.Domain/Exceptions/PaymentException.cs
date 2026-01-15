using System;

namespace PaymentsMS.Domain.Exceptions
{
    public class PaymentException : DomainException
    {
        public PaymentException(string message) : base(message) { }
    }
}
