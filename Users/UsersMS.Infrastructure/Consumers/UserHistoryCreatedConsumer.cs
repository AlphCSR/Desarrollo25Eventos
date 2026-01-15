using MassTransit;
using Microsoft.Extensions.Logging;
using UsersMS.Domain.Entities;
using UsersMS.Domain.Interfaces;
using UsersMS.Shared.Events;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace UsersMS.Infrastructure.Consumers
{
    public class UserHistoryCreatedConsumer : IConsumer<UserHistoryCreatedEvent>
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserHistoryCreatedConsumer> _logger;

        public UserHistoryCreatedConsumer(IUserRepository repository, ILogger<UserHistoryCreatedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserHistoryCreatedEvent> context)
        {
            var message = context.Message;
            Guid targetUserId = message.UserId;
            var user = await _repository.GetByIdAsync(targetUserId, context.CancellationToken);
            
            if (user == null)
            {
                user = await _repository.GetByKeycloakIdAsync(message.UserId.ToString(), context.CancellationToken);
                if (user != null)
                {
                    targetUserId = user.Id;
                    _logger.LogInformation($"Resolviendo el ID Interno del Usuario {message.UserId} (KeycloakId) a ID Interno {targetUserId}");
                }
            }

            if (user == null)
            {
                _logger.LogWarning($"Usuario con ID {message.UserId} no encontrado (verificado como ID Interno y KeycloakId). Omitiendo la creacion del Historial de Usuario.");
                return;
            }

            var sanitizedDetails = SanitizeDetails(message.Details);
            var history = new UserHistory(targetUserId, message.Action, sanitizedDetails, message.OccurredOn, message.FriendlyMessage);
            
            await _repository.AddHistoryAsync(history, context.CancellationToken);
            await _repository.SaveChangesAsync(context.CancellationToken);
            
            _logger.LogInformation("Historial de usuario guardado exitosamente.");
        }

        private string SanitizeDetails(string details)
        {
            if (string.IsNullOrWhiteSpace(details)) return details;

            try
            {
                var jsonNode = JsonNode.Parse(details);
                if (jsonNode is JsonObject jsonObj)
                {
                    SanitizeNode(jsonObj);
                    return jsonObj.ToJsonString();
                }
            }
            catch (JsonException)
            {
            }

            return details;
        }

        private void SanitizeNode(JsonObject node)
        {
            foreach (var property in node.ToList())
            {
                if (property.Key.Contains("Password", StringComparison.OrdinalIgnoreCase))
                {
                    node[property.Key] = "******";
                }
                else if (property.Value is JsonObject childObject)
                {
                    SanitizeNode(childObject);
                }
                else if (property.Value is JsonArray childArray)
                {
                    foreach (var item in childArray)
                    {
                        if (item is JsonObject arrayObject)
                        {
                            SanitizeNode(arrayObject);
                        }
                    }
                }
            }
        }
    }
}
