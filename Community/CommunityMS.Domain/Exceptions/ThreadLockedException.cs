namespace CommunityMS.Domain.Exceptions
{
    public class ThreadLockedException : DomainException
    {
        public ThreadLockedException() : base("No se puede responder a un hilo que está bloqueado.")
        {
        }
    }
}
