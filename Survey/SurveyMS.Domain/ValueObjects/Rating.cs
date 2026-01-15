using SurveyMS.Domain.Exceptions;

namespace SurveyMS.Domain.ValueObjects
{
    public record Rating
    {
        public int Value { get; private set; }

        private Rating(int value)
        {
            Value = value;
        }

        public static Rating Create(int value)
        {
            if (value < 1 || value > 5)
                throw new InvalidSurveyDataException("La calificación debe estar entre 1 y 5.");

            return new Rating(value);
        }

        public static implicit operator int(Rating rating) => rating.Value;
        public static explicit operator Rating(int value) => Create(value);
    }
}
