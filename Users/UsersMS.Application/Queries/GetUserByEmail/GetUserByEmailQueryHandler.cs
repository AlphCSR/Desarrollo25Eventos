using MediatR;
using UsersMS.Application.DTOs;
using UsersMS.Domain.Interfaces;

namespace UsersMS.Application.Queries.GetUserByEmail
{
    public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByEmailQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null) throw new KeyNotFoundException("User not found");
            return new UserDto(user.Id, user.FullName, user.Email, user.KeycloakId, user.Role, user.State);
        }
    }
}
