using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Persistence;
using UpperCube.Web.Areas.Admin.Models.Logs;

namespace UpperCube.Web.Areas.Admin.Controllers;

public sealed class LogsController(
    IAuditLogStore auditLogStore,
    IErrorLogStore errorLogStore,
    ILogger<LogsController> logger) : AdminControllerBase
{
    public async Task<IActionResult> Audit(CancellationToken ct)
    {
        var model = new AdminAuditLogsViewModel();

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            model.Items = await auditLogStore.GetRecentAsync(100, timeout.Token);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load admin audit logs.");
            model.Warning = "Audit logs сейчас недоступны. Проверь MongoDB.";
        }

        return View(model);
    }

    public async Task<IActionResult> Errors(CancellationToken ct)
    {
        var model = new AdminErrorLogsViewModel();

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            model.Items = await errorLogStore.GetRecentAsync(100, timeout.Token);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load admin error logs.");
            model.Warning = "Error logs сейчас недоступны. Проверь MongoDB.";
        }

        return View(model);
    }
}
