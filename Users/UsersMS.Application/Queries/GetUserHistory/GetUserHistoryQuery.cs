
using MediatR;
using System;
using System.Collections.Generic;
using UsersMS.Application.DTOs;

namespace UsersMS.Application.Queries.GetUserHistory
{
    public record GetUserHistoryQuery(Guid UserId) : IRequest<List<UserHistoryDto>>;
}
