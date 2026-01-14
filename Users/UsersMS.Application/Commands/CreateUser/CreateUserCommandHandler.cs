using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Commands;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Application.Interfaces;
using UsersMS.Shared.Enums;
using UsersMS.Domain.Exceptions;
using MassTransit;
using UsersMS.Shared.Events;

namespace UsersMS.Application.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakService _keycloakService;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateUserCommandHandler(IUserRepository userRepository, IKeycloakService keycloakService, IPublishEndpoint publishEndpoint)
        {
            _userRepository = userRepository;
            _keycloakService = keycloakService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.UserData.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException($"El usuario con email {request.UserData.Email} ya existe.");
            }

            var names = request.UserData.FullName.Split(' ', 2);
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[1] : "";

            string keycloakId;
            if (string.IsNullOrEmpty(request.UserData.KeycloakId))
            {
                keycloakId = await _keycloakService.RegisterUserAsync(
                    request.UserData.Email,
                    request.UserData.Password,
                    firstName,
                    lastName,
                    cancellationToken);

                await _keycloakService.AssignRoleAsync(request.UserData.Email, request.UserData.Role.ToString().ToLower(), cancellationToken);
            }
            else
            {
                keycloakId = request.UserData.KeycloakId;
            }

            var newUser = new User(
                request.UserData.FullName,
                request.UserData.Email,
                keycloakId,
                request.UserData.Role
            );

            await _userRepository.AddAsync(newUser, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new UserHistoryCreatedEvent(
                newUser.Id,
                "UserCreated",
                $"Usuario creado con rol {request.UserData.Role}",
                DateTime.UtcNow
            ), cancellationToken);

            return newUser.Id;
        }
    }
}