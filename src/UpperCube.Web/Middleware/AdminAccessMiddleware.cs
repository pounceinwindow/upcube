using Microsoft.AspNetCore.Http.Extensions;

namespace UpperCube.Web.Middleware;

public sealed class AdminAccessMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
            return next(context);

        if (context.User.Identity?.IsAuthenticated != true)
        {
            var returnUrl = context.Request.GetEncodedPathAndQuery();
            context.Response.Redirect($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            return Task.CompletedTask;
        }

        if (!context.User.IsInRole("Admin"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        return next(context);
    }
}