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
        await PropertySelectionViewDataHelper.PopulateAsync(
            controller,
            userManager,
            "CompareIds",
            comparisonRepository.GetUserComparisonPropertyIdsAsync,
            ct);
    }
}
