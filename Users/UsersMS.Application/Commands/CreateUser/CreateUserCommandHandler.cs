using MediatR;
using System;
using UsersMS.Application.DTOs;
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;

namespace UsersMS.Application.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakService _keycloakService;

        public CreateUserCommandHandler(IUserRepository userRepository, IKeycloakService keycloakService)
        {
            _userRepository = userRepository;
            _keycloakService = keycloakService;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.UserData.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"El usuario con email {request.UserData.Email} ya existe.");
            }

            var names = request.UserData.FullName.Split(' ', 2);
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[1] : "";

            string keycloakId = await _keycloakService.RegisterUserAsync(
                request.UserData.Email, 
                request.UserData.Password, 
                firstName, 
                lastName, 
                cancellationToken);

            await _keycloakService.AssignRoleAsync(request.UserData.Email, request.UserData.Role.ToString(), cancellationToken);

            var newUser = new User(
                request.UserData.FullName,
                request.UserData.Email,
                keycloakId,
                request.UserData.Role
            );

            await _userRepository.AddAsync(newUser, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return newUser.Id;
        }
    }
}