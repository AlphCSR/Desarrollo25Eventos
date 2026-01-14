using System.Text.RegularExpressions;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa un correo electrónico.
    /// </summary>
    public record Email
    {
        /// <summary>
        /// Valor del correo electrónico.
        /// </summary>
        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Crea una nueva instancia de <see cref="Email"/>.
        /// </summary>
        /// <param name="email">Dirección de correo electrónico.</param>
        /// <returns>Una nueva instancia de <see cref="Email"/>.</returns>
        /// <exception cref="InvalidUserDataException">Lanzada si el email es vacío o tiene un formato inválido.</exception>
        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidUserDataException("El email no puede estar vacío.");

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new InvalidUserDataException("El formato del email es inválido.");

            return new Email(email);
        }

        public static implicit operator string(Email email) => email.Value;
        public static explicit operator Email(string email) => Create(email);
    }
}