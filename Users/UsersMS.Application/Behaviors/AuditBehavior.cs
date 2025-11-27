using MediatR;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UsersMS.Application.Interfaces;
using UsersMS.Domain.Entities;

namespace UsersMS.Application.Behaviors;

/// <summary>
/// Comportamiento de auditoría para mediatr.
/// </summary>
[ExcludeFromCodeCoverage]
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

    /// <summary>
    /// Maneja la auditoría de los comandos.
    /// </summary>
    /// <param name="request">El comando a ejecutar.</param>
    /// <param name="next">El delegado del siguiente handler.</param>
    /// <param name="cancellationToken">El token de cancelación.</param>
    /// <returns>El resultado del comando.</returns>
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
            Payload = JsonSerializer.Serialize(request)
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
}