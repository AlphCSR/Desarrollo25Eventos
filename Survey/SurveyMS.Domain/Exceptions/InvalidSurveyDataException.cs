using System;

namespace SurveyMS.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }
    }

    public class InvalidSurveyDataException : DomainException
    {
        public InvalidSurveyDataException(string message) : base(message)
        {
        }
    }
}
