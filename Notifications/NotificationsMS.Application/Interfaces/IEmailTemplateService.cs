namespace NotificationsMS.Application.Interfaces
{
    public interface IEmailTemplateService
    {
        (string Subject, string Body) GetBookingConfirmedTemplate(string language, string bookingId, string eventName);
        (string Subject, string Body) GetBookingCancelledTemplate(string language, string bookingId, string eventName);
        (string Subject, string Body) GetEventCancelledTemplate(string language, string eventName);
    }
}
