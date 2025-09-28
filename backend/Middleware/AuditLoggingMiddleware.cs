using System.Security.Claims;
using AdminPanel.Api.Data;
using AdminPanel.Api.Models;
using Microsoft.AspNetCore.Http.Extensions;

namespace AdminPanel.Api.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, DataContext dataContext)
    {
        await _next(context);

        if (!IsReleventRequest(context)) return;

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) ??
            context.User.FindFirst("sub");

        if (userIdClaim == null)
        {
            _logger.LogInformation("Cannot log audit: no user ID found.");
            return;
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Invalid user ID in claims: {userId}", userIdClaim.Value);
            return;
        }

        var user = await dataContext.Users.FindAsync(userId);
        if (user is null)
        {
            _logger.LogInformation("user not found for ID: {userId}", userId);
            return;
        }

        var action = GetActionFromMethodAndPath(context.Request.Method, context.Request.Path);

        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            Details = $"Endpoint: {context.Request.GetDisplayUrl()} | IP: {GetClientIp(context)}"
        };

        try
        {
            dataContext.AuditLogs.Add(log);
            await dataContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save audit log entry.");
        }
    }

    private bool IsReleventRequest(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.ToString().ToLower();

        return context.Response.StatusCode == 200 &&
            (method == "POST" || method == "PUT" || method == "DELETE") &&
            path.Contains("/api");
    }

    private string GetActionFromMethodAndPath(string method, string path)
    {
        return path switch
        {
            string p when p.Contains("/api/users") && method == "POST" => "UserCreated",
            string p when p.Contains("/api/users") && method == "PUT" => "UserUpdated",
            string p when p.Contains("/api/users") && method == "DELETE" => "UserDeleted",
            string p when p.Contains("/api/roles") && method == "POST" => "RoleCreated",
            string p when p.Contains("/api/roles") && method == "PUT" => "RoleUpdated",
            _ => $"{method.ToUpper()}_{path}"
        };
    }

    private object GetClientIp(HttpContext context)
    {
        return context.Request.Headers["X-Forward-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }
}
