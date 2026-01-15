using System;

namespace UsersMS.Shared.Events
{
    public record UserHistoryCreatedEvent(Guid UserId, string Action, string Details, DateTime OccurredOn, string FriendlyMessage = "");
}
