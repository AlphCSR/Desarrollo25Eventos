using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace UsersMS.Application.DTOs
{
    [ExcludeFromCodeCoverage]
    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public List<string> Preferences { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DocumentId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Language { get; set; }

        public UpdateUserDto() { }

        public UpdateUserDto(string fullName)
        {
            FullName = fullName;
        }
    }
}