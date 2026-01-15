using MarketingMS.Domain.Exceptions;

namespace MarketingMS.Domain.ValueObjects
{
    public record CouponCode
    {
        public string Value { get; private set; }

        private CouponCode(string value)
        {
            Value = value;
        }

        public static CouponCode Create(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidMarketingDataException("El código de cupón no puede estar vacío.");

            return new CouponCode(code.ToUpper().Trim());
        }

        public static implicit operator string(CouponCode code) => code.Value;
        public static explicit operator CouponCode(string code) => Create(code);

        public override string ToString() => Value;
    }
}
