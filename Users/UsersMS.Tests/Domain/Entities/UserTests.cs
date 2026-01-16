using FluentAssertions;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Exceptions;
using UsersMS.Domain.ValueObjects;
using UsersMS.Shared.Enums;
using Xunit;

namespace UsersMS.Tests.Domain.Entities
{
    public class UserTests
    {
        [Fact]
        public void Constructor_Tests()
        {
            // Case 1: Valid construction
            var user = new User("Juan Perez", "juan@example.com", "kc-123", UserRole.User);
            user.FullName.Value.Should().Be("Juan Perez");
            user.Email.Value.Should().Be("juan@example.com");
            user.KeycloakId.Should().Be("kc-123");
            user.Role.Should().Be(UserRole.User);
            user.State.Should().Be(UserState.Active);

            // Case 2: Missing KeycloakId
            Action act = () => new User("Juan Perez", "juan@example.com", "", UserRole.User);
            act.Should().Throw<InvalidUserDataException>().WithMessage("El ID de Keycloak es requerido.");
        }

        [Fact]
        public void UpdatePreferences_Tests()
        {
            var user = new User("Juan Perez", "juan@example.com", "kc-123", UserRole.User);
            
            // Valid case
            var newPrefs = new List<string> { "email", "sms" };
            user.UpdatePreferences(newPrefs);
            user.Preferences.Should().BeEquivalentTo(newPrefs);

            // Error case: Null list
            Action act = () => user.UpdatePreferences(null!);
            act.Should().Throw<InvalidUserDataException>().WithMessage("La lista de preferencias no puede ser nula.");
        }

        [Fact]
        public void UpdateProfile_Tests()
        {
            var user = new User("Juan Perez", "juan@example.com", "kc-123", UserRole.User);
            
            // Valid update
            user.UpdateProfile("Juan Actualizado", "+34600111222", "12345678Z", new DateTime(1990, 1, 1), "Calle Falsa 123", "http://image.com/p.jpg", "en");
            
            user.FullName.Value.Should().Be("Juan Actualizado");
            user.PhoneNumber!.Value.Should().Be("+34600111222");
            user.DocumentId.Should().Be("12345678Z");
            user.Language.Should().Be("en");
            user.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Deactivate_Tests()
        {
            var user = new User("Juan Perez", "juan@example.com", "kc-123", UserRole.User);
            user.State.Should().Be(UserState.Active);

            user.Deactivate();
            user.State.Should().Be(UserState.Inactive);
            user.UpdatedAt.Should().NotBeNull();
        }
    }
}
