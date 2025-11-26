using System;
using UsersMS.Domain.Common;

namespace UsersMS.Domain.Entities
{
    public class UserHistory : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Action { get; private set; }
        public string Details { get; private set; }
        
        protected UserHistory() { }

        public UserHistory(Guid userId, string action, string details)
        {
            UserId = userId;
            Action = action;
            Details = details;
        }
    }
}