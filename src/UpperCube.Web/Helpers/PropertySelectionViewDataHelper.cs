using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Infrastructure.Identity;

namespace UpperCube.Web.Helpers;

internal static class PropertySelectionViewDataHelper
{
    public static async Task PopulateAsync(
        Controller controller,
        UserManager<ApplicationUser> userManager,
        string viewDataKey,
        Func<string, CancellationToken, Task<IReadOnlyList<int>>> loadIdsAsync,
        CancellationToken ct = default)
    {
        if (controller.User.Identity?.IsAuthenticated != true)
        {
            controller.ViewData[viewDataKey] = new HashSet<int>();
            return;
        }

        var userId = userManager.GetUserId(controller.User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            controller.ViewData[viewDataKey] = new HashSet<int>();
            return;
        }

        var ids = await loadIdsAsync(userId, ct);
        controller.ViewData[viewDataKey] = ids.ToHashSet();
    }
}
