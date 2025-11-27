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
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Exceptions;

namespace UsersMS.Infrastructure.Services
{
    /// <summary>
    /// Servicio para interactuar con la API de Keycloak.
    /// </summary>
    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public KeycloakService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        /// <summary>
        /// Registra un nuevo usuario en Keycloak.
        /// </summary>
        /// <param name="email">Email del usuario.</param>
        /// <param name="password">Contraseña del usuario.</param>
        /// <param name="firstName">Nombre del usuario.</param>
        /// <param name="lastName">Apellido del usuario.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>ID del usuario creado en Keycloak.</returns>
        /// <exception cref="KeycloakIntegrationException">Lanzada si ocurre un error en la integración con Keycloak.</exception>
        public async Task<string> RegisterUserAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken)
        {
            // 1. Obtener Token de Administrador
            var token = await GetAdminAccessTokenAsync(cancellationToken);

            // 2. Preparar payload de creación
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

            // 3. Enviar petición a Keycloak
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsJsonAsync($"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users", newUser, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new KeycloakIntegrationException($"Error al crear usuario en Keycloak: {error}");
            }

            // 4. Obtener el ID del usuario recién creado (Keycloak retorna Location header)
            var locationHeader = response.Headers.Location;
            if (locationHeader == null) throw new KeycloakIntegrationException("Keycloak no devolvió la ubicación del usuario creado.");

            // El ID es la última parte de la URL
            var pathSegments = locationHeader.ToString().Split('/');
            return pathSegments[pathSegments.Length - 1];
        }

        private async Task<string> GetAdminAccessTokenAsync(CancellationToken cancellationToken)
        {
            // 1. Obtener token de administrador    
            var tokenUrl = $"{_configuration["Keycloak:AuthServerUrl"]}/realms/{_configuration["Keycloak:Realm"]}/protocol/openid-connect/token";
            
            // 2. Preparar payload de autenticación
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _configuration["Keycloak:ClientId"] ?? throw new KeycloakIntegrationException("No se pudo obtener el token de admin.")),
                new KeyValuePair<string, string>("client_secret", _configuration["Keycloak:ClientSecret"] ?? throw new KeycloakIntegrationException("No se pudo obtener el token de admin.")),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            // 3. Enviar petición de autenticación
            var response = await _httpClient.PostAsync(tokenUrl, requestContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            // 4. Obtener el token de administrador
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JObject.Parse(content);
            return json["access_token"]?.ToString() ?? throw new KeycloakIntegrationException("No se pudo obtener el token de admin.");
        }

        /// <summary>
        /// Asigna un rol a un usuario en Keycloak.
        /// </summary>
        /// <param name="username">Nombre de usuario (email).</param>
        /// <param name="role">Nombre del rol a asignar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <exception cref="KeycloakIntegrationException">Lanzada si ocurre un error en la integración con Keycloak.</exception>
        public async Task AssignRoleAsync(string username, string role, CancellationToken cancellationToken)
        {
            var token = await GetAdminAccessTokenAsync(cancellationToken);
            var realm = _configuration["Keycloak:Realm"];

            // Paso 1: Buscar al usuario
            var userSearchResponse = await _httpClient.GetAsync($"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users?username={username}", cancellationToken);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

            // Paso 2: Buscar el cliente
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

            // Paso 3: Buscar el rol
            var roleResponse = await _httpClient.GetAsync($"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/clients/{clientId}/roles/{role}", cancellationToken);
            if (!roleResponse.IsSuccessStatusCode)
            {
                var error = await roleResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new KeycloakIntegrationException($"No se pudo encontrar el rol '{role}' en Keycloak. Respuesta: {error}");
            }

            var roleContent = await roleResponse.Content.ReadAsStringAsync(cancellationToken);
            var roleJson = JsonSerializer.Deserialize<JsonElement>(roleContent);

            // Paso 4: Asignar el rol al usuario
            var assignRoleRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users/{userId}/role-mappings/clients/{clientId}"
            );

            assignRoleRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            assignRoleRequest.Content = new StringContent(JsonSerializer.Serialize(new[] { roleJson }), Encoding.UTF8, "application/json");

            var assignRoleResponse = await _httpClient.SendAsync(assignRoleRequest, cancellationToken);

            if (!assignRoleResponse.IsSuccessStatusCode)
            {
                var error = await assignRoleResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new KeycloakIntegrationException($"No se pudo asignar el rol '{role}' al usuario '{username}'. Respuesta: {error}");
            }
        }

        /// <summary>
        /// Actualiza los datos de un usuario en Keycloak.
        /// </summary>
        /// <param name="keycloakId">ID de Keycloak del usuario.</param>
        /// <param name="firstName">Nuevo nombre.</param>
        /// <param name="lastName">Nuevo apellido.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task UpdateUserAsync(string keycloakId, string firstName, string lastName, CancellationToken cancellationToken)
        {
            var token = await GetAdminAccessTokenAsync(cancellationToken);
            var realm = _configuration["Keycloak:Realm"];

            var updateData = new
            {
                firstName = firstName,
                lastName = lastName
            };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            // PUT /admin/realms/{realm}/users/{id}
            var response = await _httpClient.PutAsJsonAsync(
                $"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users/{keycloakId}", 
                updateData, 
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Desactiva un usuario en Keycloak.
        /// </summary>
        /// <param name="keycloakId">ID de Keycloak del usuario.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task DeactivateUserAsync(string keycloakId, CancellationToken cancellationToken)
        {
            var token = await GetAdminAccessTokenAsync(cancellationToken);
            var realm = _configuration["Keycloak:Realm"];

            var updateData = new
            {
                enabled = false // Deshabilitamos al usuario
            };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await _httpClient.PutAsJsonAsync(
                $"{_configuration["Keycloak:AuthServerUrl"]}/admin/realms/{realm}/users/{keycloakId}", 
                updateData, 
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }
}