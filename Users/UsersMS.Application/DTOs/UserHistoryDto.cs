
using System;
using UsersMS.Domain.Entities;

namespace UsersMS.Application.DTOs
{
    public record UserHistoryDto(string Action, string Details, DateTime Timestamp);
}
