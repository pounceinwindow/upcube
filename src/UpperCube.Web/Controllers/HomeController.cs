using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Helpers;
using UpperCube.Web.Mapping;

namespace UpperCube.Web.Controllers;

public sealed class HomeController(
    IPropertyRepository repository,
    IFavoriteRepository favoriteRepository,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = (await repository
                .GetLatestPublishedAsync(6, ct))
            .Select(p => p.ToListItemDto());

        await FavoritesViewDataHelper.PopulateFavoriteIdsAsync(this, favoriteRepository, userManager, ct);
        return View(model);
    }
}