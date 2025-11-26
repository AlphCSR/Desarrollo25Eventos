using MediatR;
using UsersMS.Application.DTOs;
using UsersMS.Domain.Interfaces;

namespace UsersMS.Application.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
    {
        private readonly IUserRepository _repository;

        public GetAllUsersQueryHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _repository.GetAllAsync(cancellationToken);
            return users.Select(user => new UserDto(user.Id, user.FullName, user.Email, user.KeycloakId, user.Role, user.State));
        }
    }
}