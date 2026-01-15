using System;

namespace PaymentsMS.Application.DTOs
{
    public record CreatePaymentDto(Guid BookingId, Guid UserId, decimal Amount, string Currency, string? Email = null);
    public record PaymentIntentResponseDto(string ClientSecret, string StripePaymentIntentId);
}
