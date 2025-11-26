using MediatR;
using UsersMS.Application.DTOs;
using UsersMS.Domain.Interfaces;

namespace UsersMS.Application.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly IUserRepository _repository;

        public GetUserByIdQueryHandler(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null) return null;

            return new UserDto(user.Id, user.FullName, user.Email, user.KeycloakId, user.Role, user.State);
        }
    }
}