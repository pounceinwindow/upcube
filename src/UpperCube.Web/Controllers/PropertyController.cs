using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Helpers;
using UpperCube.Web.Mapping;

namespace UpperCube.Web.Controllers;

public sealed class PropertyController(
    IPropertyRepository repository,
    IFavoriteRepository favoriteRepository,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var model = await repository
            .GetByIdAsync(id, ct);

        if (model == null) return NotFound();

        await repository.IncrementViewsAsync(id, ct);

        var details = model.ToDetailsDto();

        await FavoritesViewDataHelper.PopulateFavoriteIdsAsync(this, favoriteRepository, userManager, ct);
        return View(details);
    }
}