using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpperCube.Application.Abstractions.Persistence;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Enums;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Areas.Admin.Models.Dashboard;

namespace UpperCube.Web.Areas.Admin.Controllers;

public sealed class DashboardController(
    UserManager<ApplicationUser> userManager,
    IPropertyRepository propertyRepository,
    IInquiryRepository inquiryRepository,
    IValuationRepository valuationRepository,
    IFeatureCatalogRepository featureCatalogRepository,
    IAuditLogStore auditLogStore,
    IErrorLogStore errorLogStore,
    ILogger<DashboardController> logger) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = new AdminDashboardViewModel
        {
            UsersCount = await userManager.Users.CountAsync(ct),
            PropertiesCount = await propertyRepository.CountAsync(ct),
            PendingPropertiesCount = await propertyRepository.CountByStatusAsync(PropertyStatus.PendingModeration, ct),
            InquiriesCount = await inquiryRepository.CountAsync(ct),
            ValuationsCount = await valuationRepository.CountAsync(ct),
            FeatureCatalogCount = await featureCatalogRepository.CountAsync(ct),
            RecentAuditLogsCount = await CountRecentAuditLogsAsync(ct),
            RecentErrorsCount = await CountRecentErrorsAsync(ct)
        };

        return View(model);
    }

    private async Task<int> CountRecentAuditLogsAsync(CancellationToken ct)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            var logs = await auditLogStore.GetRecentAsync(20, timeout.Token);
            return logs.Count;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load recent audit logs count for admin dashboard.");
            return 0;
        }
    }

    private async Task<int> CountRecentErrorsAsync(CancellationToken ct)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));

            var logs = await errorLogStore.GetRecentAsync(20, timeout.Token);
            return logs.Count;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load recent error logs count for admin dashboard.");
            return 0;
        }
    }
}
