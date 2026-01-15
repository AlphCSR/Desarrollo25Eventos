using System;

namespace InvoicingMS.Domain.ValueObjects
{
    public record Money
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money Create(decimal amount, string currency = "USD")
        {
            if (amount < 0)
                throw new ArgumentException("El monto no puede ser negativo.");

            return new Money(amount, currency);
        }

        public static implicit operator decimal(Money money) => money.Amount;
        public static explicit operator Money(decimal amount) => Create(amount);

        public override string ToString() => $"{Amount} {Currency}";
    }
}
