using System;

namespace BookingMS.Shared.Events;

public class PaymentCapturedEvent
{
    public Guid BookingId { get; set; }
    public Guid TransactionId { get; set; }
}
