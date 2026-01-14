using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Infrastructure.Services
{
    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public KeycloakService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> RegisterUserAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken)
        {
            var token = await GetAdminAccessTokenAsync(cancellationToken);

            var realm = _configuration["Keycloak:Realm"];
            var newUser = new
            {
                username = email,
                email = email,
                firstName = firstName,
                lastName = lastName,
                enabled = true,
                credentials = new[]
                {
                    new { type = "password", value = password, temporary = false }
                }
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsJsonAsync($"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users", newUser, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new KeycloakIntegrationException($"Error al crear usuario en Keycloak: {error}");
            }

            var locationHeader = response.Headers.Location;
            if (locationHeader == null) throw new KeycloakIntegrationException("Keycloak no devolvió la ubicación del usuario creado.");

            var pathSegments = locationHeader.ToString().Split('/');
            return pathSegments[pathSegments.Length - 1];
        }

        private async Task<string> GetAdminAccessTokenAsync(CancellationToken cancellationToken)
        {
            var tokenUrl = $"{_configuration["Keycloak:AuthServerUrl"]}/realms/master/protocol/openid-connect/token";
            
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "admin-cli"),
                new KeyValuePair<string, string>("username", _configuration["Keycloak:AdminUsername"] ?? "admin"),
                new KeyValuePair<string, string>("password", _configuration["Keycloak:AdminPassword"] ?? "admin"),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            var response = await _httpClient.PostAsync(tokenUrl, requestContent, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new KeycloakIntegrationException($"No se pudo obtener el token de admin. Status: {response.StatusCode}. Detalles: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JObject.Parse(content);
            return json["access_token"]?.ToString() ?? throw new KeycloakIntegrationException("No se pudo obtener el token de admin.");
        }

        public async Task AssignRoleAsync(string username, string role, CancellationToken cancellationToken)
        {
            var token = await GetAdminAccessTokenAsync(cancellationToken);
            var realm = _configuration["Keycloak:Realm"];

            var userSearchResponse = await _httpClient.GetAsync($"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users?username={username}", cancellationToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (!userSearchResponse.IsSuccessStatusCode)
            {
                var error = await userSearchResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new KeycloakIntegrationException($"No se pudo encontrar el usuario '{username}' en Keycloak. Respuesta: {error}");
            }

            var userContent = await userSearchResponse.Content.ReadAsStringAsync(cancellationToken);
            var userArray = JsonSerializer.Deserialize<JsonElement>(userContent);
            if (userArray.GetArrayLength() == 0)
                throw new KeycloakIntegrationException($"Usuario '{username}' no encontrado en Keycloak.");

            var userId = userArray[0].GetProperty("id").GetString();

            var clientResponse = await _httpClient.GetAsync($"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/clients?clientId={_configuration["Keycloak:ClientId"]}", cancellationToken);
            if (!clientResponse.IsSuccessStatusCode)
            {
                var error = await clientResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new KeycloakIntegrationException($"No se pudo encontrar el cliente '{_configuration["Keycloak:ClientId"]}' en Keycloak. Respuesta: {error}");
            }

            var clientContent = await clientResponse.Content.ReadAsStringAsync(cancellationToken);
            var clientArray = JsonSerializer.Deserialize<JsonElement>(clientContent);
            if (clientArray.GetArrayLength() == 0)
                throw new KeycloakIntegrationException($"Cliente '{_configuration["Keycloak:ClientId"]}' no encontrado.");

            var clientId = clientArray[0].GetProperty("id").GetString();

            var roleResponse = await _httpClient.GetAsync($"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/clients/{clientId}/roles/{role}", cancellationToken);
            if (!roleResponse.IsSuccessStatusCode)
            {
                var error = await roleResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new KeycloakIntegrationException($"No se pudo encontrar el rol '{role}' en Keycloak. Respuesta: {error}");
            }

            var roleContent = await roleResponse.Content.ReadAsStringAsync(cancellationToken);
            var roleJson = JsonSerializer.Deserialize<JsonElement>(roleContent);

            var assignRoleRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users/{userId}/role-mappings/clients/{clientId}"
            );

            assignRoleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            assignRoleRequest.Content = new StringContent(JsonSerializer.Serialize(new[] { roleJson }), Encoding.UTF8, "application/json");

            var assignRoleResponse = await _httpClient.SendAsync(assignRoleRequest, cancellationToken);

            if (!assignRoleResponse.IsSuccessStatusCode)
            {
                var error = await assignRoleResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new KeycloakIntegrationException($"No se pudo asignar el rol '{role}' al usuario '{username}'. Respuesta: {error}");
            }
        }

        public async Task UpdateUserAsync(string keycloakId, string firstName, string lastName, CancellationToken cancellationToken)
        {
            var token = await GetAdminAccessTokenAsync(cancellationToken);
            var realm = _configuration["Keycloak:Realm"];

            var updateData = new
            {
                firstName = firstName,
                lastName = lastName
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await _httpClient.PutAsJsonAsync(
                $"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users/{keycloakId}", 
                updateData, 
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeactivateUserAsync(string keycloakId, CancellationToken cancellationToken)
        {
            var token = await GetAdminAccessTokenAsync(cancellationToken);
            var realm = _configuration["Keycloak:Realm"];

            var updateData = new
            {
                enabled = false 
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await _httpClient.PutAsJsonAsync(
                $"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users/{keycloakId}", 
                updateData, 
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }
}