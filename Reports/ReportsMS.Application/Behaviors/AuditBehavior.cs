using MediatR;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ReportsMS.Application.Interfaces;
using ReportsMS.Domain.Entities;

namespace ReportsMS.Application.Behaviors;

public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditBehavior(IAuditService auditService, IHttpContextAccessor httpContextAccessor)
    {
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "Anonymous";
        var requestName = typeof(TRequest).Name;
        
        if (!requestName.EndsWith("Command")) 
        {
            return await next();
        }

        var log = new AuditLog
        {
            UserId = userId,
            Action = requestName,
            Payload = SanitizePayload(request)
        };

        try
        {
            var response = await next(); 
            log.IsSuccess = true;
            return response;
        }
        catch (Exception ex)
        {
            log.IsSuccess = false;
            log.ErrorMessage = ex.Message;
            throw; 
        }
        finally
        {
            stopwatch.Stop();
            await _auditService.LogAsync(log);
        }
    }

    private string SanitizePayload(object request)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(request);
            var jsonNode = JsonNode.Parse(jsonString);

            if (jsonNode is JsonObject jsonObj)
            {
                SanitizeNode(jsonObj);
            }

            return jsonNode?.ToJsonString() ?? "{}";
        }
        catch
        {
            return "Error serializando el payload";
        }
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
