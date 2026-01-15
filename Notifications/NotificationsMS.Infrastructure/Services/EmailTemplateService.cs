using NotificationsMS.Application.Interfaces;

namespace NotificationsMS.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public (string Subject, string Body) GetBookingConfirmedTemplate(string language, string bookingId, string eventName)
        {
            if (language?.ToLower() == "en")
            {
                return (
                    "Booking Confirmation - TicketApp",
                    $"Hello! Your booking {bookingId} for the event '{eventName}' has been successfully confirmed. Enjoy the show!"
                );
            }

            return (
                "Confirmación de Reserva - TicketApp",
                $"¡Hola! Tu reserva {bookingId} para el evento '{eventName}' ha sido confirmada exitosamente. ¡Disfruta el evento!"
            );
        }

        public (string Subject, string Body) GetBookingCancelledTemplate(string language, string bookingId, string eventName)
        {
            if (language?.ToLower() == "en")
            {
                return (
                    "Booking Cancelled - TicketApp",
                    $"We inform you that your booking {bookingId} for the event '{eventName}' has been cancelled. If you have questions, please contact support."
                );
            }

            return (
                "Reserva Cancelada - TicketApp",
                $"Te informamos que tu reserva {bookingId} para el evento '{eventName}' ha sido cancelada. Si tienes dudas, contacta a soporte."
            );
        }

        public (string Subject, string Body) GetEventCancelledTemplate(string language, string eventName)
        {
            if (language?.ToLower() == "en")
            {
                return (
                    "Event Cancelled - Important Notice",
                    $"We regret to inform you that the event '{eventName}' has been cancelled by the organizer. A refund will be processed according to our policies."
                );
            }

            return (
                "Evento Cancelado - Aviso Importante",
                $"Lamentamos informarte que el evento '{eventName}' ha sido cancelado por el organizador. Se procesará un reembolso según nuestras políticas."
            );
        }
    }
}
