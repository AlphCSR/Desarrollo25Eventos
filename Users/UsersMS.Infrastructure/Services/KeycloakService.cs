using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using UsersMS.Application.Interfaces;

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
                throw new InvalidOperationException($"Error al crear usuario en Keycloak: {error}");
            }

            // 4. Obtener el ID del usuario recién creado (Keycloak retorna Location header)
            var locationHeader = response.Headers.Location;
            if (locationHeader == null) throw new Exception("Keycloak no devolvió la ubicación del usuario creado.");

            // El ID es la última parte de la URL
            var pathSegments = locationHeader.ToString().Split('/');
            return pathSegments[pathSegments.Length - 1];
        }

        private async Task<string> GetAdminAccessTokenAsync(CancellationToken cancellationToken)
        {
            var tokenUrl = $"{_configuration["Keycloak:AuthServerUrl"]}/realms/{_configuration["Keycloak:Realm"]}/protocol/openid-connect/token";
            
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _configuration["Keycloak:ClientId"]),
                new KeyValuePair<string, string>("client_secret", _configuration["Keycloak:ClientSecret"]),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _httpClient.PostAsync(tokenUrl, requestContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JObject.Parse(content);
            return json["access_token"]?.ToString() ?? throw new Exception("No se pudo obtener el token de admin.");
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

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            // PUT /admin/realms/{realm}/users/{id}
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