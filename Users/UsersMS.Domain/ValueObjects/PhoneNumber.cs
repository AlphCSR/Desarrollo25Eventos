using System.Text.RegularExpressions;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Domain.ValueObjects
{
    public record PhoneNumber
    {
        public string Value { get; private set; }

        private PhoneNumber(string value)
        {
            Value = value;
        }

        public static PhoneNumber? Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null; 

            if (!Regex.IsMatch(value, @"^\+?[1-9]\d{1,14}$"))
                throw new InvalidUserDataException("El formato del número de teléfono es inválido.");

            return new PhoneNumber(value);
        }

        public static implicit operator string?(PhoneNumber? phone) => phone?.Value;
        public static explicit operator PhoneNumber?(string? phone) => Create(phone);
    }
}
