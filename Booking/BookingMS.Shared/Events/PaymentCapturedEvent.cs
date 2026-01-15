using System;

namespace BookingMS.Shared.Events;

public class PaymentCapturedEvent
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
}
