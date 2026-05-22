using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Infrastructure.Identity;

namespace UpperCube.Web.Helpers;

public static class CompareViewDataHelper
{
    public static async Task PopulateCompareIdsAsync(
        Controller controller,
        IComparisonRepository comparisonRepository,
        UserManager<ApplicationUser> userManager,
        CancellationToken ct = default)
    {
        if (controller.User.Identity?.IsAuthenticated != true)
        {
            controller.ViewData["CompareIds"] = new HashSet<int>();
            return;
        }

        var userId = userManager.GetUserId(controller.User);
        if (userId is null)
        {
            controller.ViewData["CompareIds"] = new HashSet<int>();
            return;
        }

        var ids = await comparisonRepository.GetUserComparisonPropertyIdsAsync(userId, ct);
        controller.ViewData["CompareIds"] = ids.ToHashSet();
    }
}
