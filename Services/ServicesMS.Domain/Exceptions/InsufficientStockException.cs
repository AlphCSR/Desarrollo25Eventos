namespace ServicesMS.Domain.Exceptions
{
    public class InsufficientStockException : DomainException
    {
        public InsufficientStockException(string serviceName) 
            : base($"No hay stock suficiente para el servicio: {serviceName}")
        {
        }
    }
}
