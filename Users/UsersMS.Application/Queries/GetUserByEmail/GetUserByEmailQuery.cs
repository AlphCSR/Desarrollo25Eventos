using MediatR;
using System;
using UsersMS.Application.DTOs;
using UsersMS.Domain.Entities;

namespace UsersMS.Application.Queries.GetUserByEmail
{
    public class GetUserByEmailQuery : IRequest<UserDto>
    {
        public string Email { get; set; }

        public GetUserByEmailQuery(string email)
        {
            Email = email;
        }
    }
}