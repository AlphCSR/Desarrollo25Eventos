using UsersMS.Domain.Exceptions;

namespace UsersMS.Domain.ValueObjects
{
    public record PersonName
    {
        public string Value { get; private set; }

        private PersonName(string value)
        {
            Value = value;
        }

        public static PersonName Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidUserDataException("El nombre no puede estar vacío.");

            if (value.Length < 2)
                throw new InvalidUserDataException("El nombre debe tener al menos 2 caracteres.");

            return new PersonName(value);
        }

        public static implicit operator string?(PersonName? name) => name?.Value;
        public static explicit operator PersonName(string name) => Create(name);
    }
}
