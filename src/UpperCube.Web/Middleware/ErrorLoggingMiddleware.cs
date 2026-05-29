using System.Security.Claims;
using UpperCube.Application.Abstractions.Persistence;
using UpperCube.Domain.Entities;

namespace UpperCube.Web.Middleware;

public sealed class ErrorLoggingMiddleware(
    RequestDelegate next,
    ILogger<ErrorLoggingMiddleware> logger)
{
    private const string ErrorLoggedItemKey = "__UpperCube.ErrorLogged";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);

            if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError
                && !WasErrorLogged(context))
                await WriteErrorLogAsync(context, $"HTTP {context.Response.StatusCode}", null);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await WriteErrorLogAsync(context, $"{ex.GetType().Name}: {ex.Message}", ex);
            throw;
        }
    }

    private async Task WriteErrorLogAsync(HttpContext context, string message, Exception? exception)
    {
        if (WasErrorLogged(context)) return;
        context.Items[ErrorLoggedItemKey] = true;

        try
        {
            var store = context.RequestServices.GetRequiredService<IErrorLogStore>();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            var entry = new ErrorLogEntry
            {
                Message = message,
                StackTrace = exception?.ToString(),
                Url = BuildSafeUrl(context.Request),
                HttpMethod = context.Request.Method,
                UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                Severity = "Error",
                Timestamp = DateTime.UtcNow
            };

            await store.WriteAsync(entry, timeout.Token);
        }
        catch (Exception logException)
        {
            logger.LogWarning(logException, "Failed to write error log.");
        }
    }

    private static bool WasErrorLogged(HttpContext context)
    {
        return context.Items.ContainsKey(ErrorLoggedItemKey);
    }

    private static string BuildSafeUrl(HttpRequest request)
    {
        return $"{request.PathBase}{request.Path}{BuildSafeQueryString(request)}";
    }

    private static string BuildSafeQueryString(HttpRequest request)
    {
        if (request.Query.Count == 0) return string.Empty;

        var parts = request.Query.Select(x =>
        {
            var value = IsSensitiveQueryKey(x.Key) ? "***" : Uri.EscapeDataString(x.Value.ToString());
            return $"{Uri.EscapeDataString(x.Key)}={value}";
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
