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
        await PropertySelectionViewDataHelper.PopulateAsync(
            controller,
            userManager,
            "FavoriteIds",
            favoriteRepository.GetUserFavoritePropertyIdsAsync,
            ct);
    }
}
