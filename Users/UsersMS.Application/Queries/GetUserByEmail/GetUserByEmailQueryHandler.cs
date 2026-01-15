using System.Diagnostics.CodeAnalysis;
using MediatR;
using UsersMS.Application.DTOs;
using UsersMS.Domain.Interfaces;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Application.Queries.GetUserByEmail
{
    public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByEmailQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null) throw new UserNotFoundException("User not found");
            return new UserDto(
                user.Id, 
                user.FullName, 
                user.Email, 
                user.KeycloakId, 
                user.Role,  
                user.State, 
                user.PhoneNumber, 
                user.DocumentId, 
                user.DateOfBirth, 
                user.Address, 
                user.ProfilePictureUrl,
                user.Preferences ?? new List<string>(),
                user.Language ?? "es"
            );
        }
    }
}
