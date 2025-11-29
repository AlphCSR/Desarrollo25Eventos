using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Infrastructure.Services;
using UsersMS.Domain.Exceptions;
using Xunit;

namespace UsersMS.Tests.Infrastructure.Services
{
    public class KeycloakServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly KeycloakService _service;

        public KeycloakServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["Keycloak:AuthServerUrl"]).Returns("http://localhost:8080");
            _configurationMock.Setup(c => c["Keycloak:Realm"]).Returns("test-realm");
            _configurationMock.Setup(c => c["Keycloak:ClientId"]).Returns("test-client");
            _configurationMock.Setup(c => c["Keycloak:ClientSecret"]).Returns("test-secret");

            _service = new KeycloakService(_httpClient, _configurationMock.Object);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnUserId_WhenSuccessful()
        {
            // Arrange
            var tokenResponse = "{\"access_token\": \"fake-token\"}";
            var createdUserId = "user-123";

            // Mock Token Request
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponse)
                });

            // Mock Create User Request
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().Contains("/users")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Headers = { Location = new Uri($"http://localhost:8080/admin/realms/test-realm/users/{createdUserId}") }
                });

            // Act
            var result = await _service.RegisterUserAsync("test@example.com", "password", "First", "Last", CancellationToken.None);

            // Assert
            result.Should().Be(createdUserId);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrowException_WhenApiReturnsError()
        {
            // Arrange
            var tokenResponse = "{\"access_token\": \"fake-token\"}";

            // Mock Token Request
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponse)
                });

            // Mock Create User Request (Failure)
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().Contains("/users")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("User already exists")
                });

            // Act
            Func<Task> act = async () => await _service.RegisterUserAsync("test@example.com", "password", "First", "Last", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeycloakIntegrationException>()
                .WithMessage("*User already exists*");
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldSucceed_WhenApiReturnsSuccess()
        {
            // Arrange
            var tokenResponse = "{\"access_token\": \"fake-token\"}";

            // Mock Token Request
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponse)
                });

            // Mock Update User Request
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.RequestUri!.ToString().Contains("/users/")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            await _service.UpdateUserAsync("user-123", "NewFirst", "NewLast", CancellationToken.None);

        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowException_WhenApiReturnsError()
        {
            // Arrange
            var tokenResponse = "{\"access_token\": \"fake-token\"}";

            // Mock Token Request
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponse)
                });

            // Mock Update User Request (Failure)
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.RequestUri!.ToString().Contains("/users/")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act
            Func<Task> act = async () => await _service.UpdateUserAsync("user-123", "NewFirst", "NewLast", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>(); // Or KeycloakIntegrationException if wrapped, but currently EnsureSuccessStatusCode throws HttpRequestException
        }

        [Fact]
        public async Task DeactivateUserAsync_ShouldSucceed_WhenApiReturnsSuccess()
        {
            // Arrange
            var tokenResponse = "{\"access_token\": \"fake-token\"}";

            // Mock Token Request
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponse)
                });

            // Mock Deactivate User Request
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.RequestUri!.ToString().Contains("/users/") && req.Content != null),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            await _service.DeactivateUserAsync("user-123", CancellationToken.None);
        }

        [Fact]
        public async Task AssignRoleAsync_ShouldSucceed_WhenRoleAssigned()
        {
            // Arrange
            var tokenResponse = "{\"access_token\": \"fake-token\"}";
            var userSearchResponse = "[{\"id\": \"user-123\"}]";
            var clientResponse = "[{\"id\": \"client-123\"}]";
            var roleResponse = "{\"id\": \"role-123\", \"name\": \"test-role\"}";

            // Mock Token Request
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponse)
                });

            // Mock User Search
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().Contains("users?username=")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(userSearchResponse)
                });

            // Mock Client Search
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().Contains("clients?clientId=")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(clientResponse)
                });

            // Mock Role Search
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().Contains("/roles/")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(roleResponse)
                });

            // Mock Assign Role
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().Contains("/role-mappings/clients/")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            await _service.AssignRoleAsync("test@example.com", "test-role", CancellationToken.None);
        }
    }
}
