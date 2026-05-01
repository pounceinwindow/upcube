using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Infrastructure.Identity;

namespace UpperCube.Web.Helpers;

public static class FavoritesViewDataHelper
{
    public static async Task PopulateFavoriteIdsAsync(
        Controller controller,
        IFavoriteRepository favoriteRepository,
        UserManager<ApplicationUser> userManager,
        CancellationToken ct = default)
    {
        if (controller.User.Identity?.IsAuthenticated != true)
        {
            controller.ViewData["FavoriteIds"] = new HashSet<int>();
            return;
        }

        var userId = userManager.GetUserId(controller.User);
        if (userId is null)
        {
            controller.ViewData["FavoriteIds"] = new HashSet<int>();
            return;
        }

        var ids = await favoriteRepository.GetUserFavoritePropertyIdsAsync(userId, ct);
        controller.ViewData["FavoriteIds"] = ids.ToHashSet();
    }
}
