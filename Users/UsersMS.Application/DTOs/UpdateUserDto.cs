using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace UsersMS.Application.DTOs
{
    [ExcludeFromCodeCoverage]
    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public List<string> Preferences { get; set; }

        public UpdateUserDto() { }

        public UpdateUserDto(string fullName)
        {
            FullName = fullName;
        }
    }
}