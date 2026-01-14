using System;
using System.Collections.Generic;

namespace UsersMS.Shared.Events
{
    public record UserProfileUpdatedEvent(
        Guid UserId, 
        string FullName, 
        List<string> Preferences, 
        string? PhoneNumber, 
        string? DocumentId, 
        DateTime? DateOfBirth, 
        string? Address, 
        string? Language);
}
