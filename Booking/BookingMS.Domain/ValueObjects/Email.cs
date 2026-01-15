using System.Text.RegularExpressions;
using BookingMS.Domain.Exceptions;

namespace BookingMS.Domain.ValueObjects
{
    public record Email
    {
        public string Value { get; private set; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidBookingStateException("El email no puede estar vacío.");

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new InvalidBookingStateException("El formato del email es inválido.");

            return new Email(email);
        }

        public static implicit operator string(Email email) => email.Value;
        public static explicit operator Email(string email) => Create(email);
    }
}
