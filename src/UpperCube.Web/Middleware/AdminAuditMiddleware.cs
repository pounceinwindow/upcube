using System.Security.Claims;
using System.Text.Json;
using UpperCube.Application.Abstractions.Persistence;
using UpperCube.Domain.Entities;

namespace UpperCube.Web.Middleware;

public sealed class AdminAuditMiddleware(
    RequestDelegate next,
    ILogger<AdminAuditMiddleware> logger)
{
    private static readonly HashSet<string> AuditedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var shouldAudit = context.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase)
                          && AuditedMethods.Contains(context.Request.Method);
        Exception? actionException = null;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            actionException = ex;
            throw;
        }
        finally
        {
            if (shouldAudit)
                await WriteAuditLogAsync(context, actionException is null
                    ? context.Response.StatusCode
                    : StatusCodes.Status500InternalServerError);
        }
    }

    private async Task WriteAuditLogAsync(HttpContext context, int statusCode)
    {
        try
        {
            var store = context.RequestServices.GetRequiredService<IAuditLogStore>();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            var entry = new AuditLogEntry
            {
                UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName = context.User.Identity?.Name,
                Action = $"{context.Request.Method} {context.Request.Path}",
                EntityType = "AdminRequest",
                EntityId = null,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value,
                    QueryString = BuildSafeQueryString(context.Request),
                    StatusCode = statusCode
                }),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                Timestamp = DateTime.UtcNow
            };

            await store.WriteAsync(entry, timeout.Token);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write admin audit log.");
        }
    }

    private static string BuildSafeQueryString(HttpRequest request)
    {
        if (request.Query.Count == 0) return string.Empty;

        var parts = request.Query.Select(x =>
        {
            var value = IsSensitiveQueryKey(x.Key) ? "***" : x.Value.ToString();
            return $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(value)}";
        });

        return "?" + string.Join("&", parts);
    }

    private static bool IsSensitiveQueryKey(string key)
    {
        return key.Contains("password", StringComparison.OrdinalIgnoreCase)
               || key.Contains("token", StringComparison.OrdinalIgnoreCase)
               || key.Contains("secret", StringComparison.OrdinalIgnoreCase);
    }
}
