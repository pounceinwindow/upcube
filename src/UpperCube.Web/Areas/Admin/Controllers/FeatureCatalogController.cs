using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Web.Areas.Admin.Models.FeatureCatalog;

namespace UpperCube.Web.Areas.Admin.Controllers;

public sealed class FeatureCatalogController(
    IFeatureCatalogRepository featureCatalogRepository,
    IUnitOfWork unitOfWork) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var features = await featureCatalogRepository.ListAsync(ct);
        return View(new AdminFeatureCatalogViewModel { Items = features });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string code, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return NotFound();

        var feature = await featureCatalogRepository.GetByCodeAsync(code, ct);
        if (feature is null) return NotFound();

        feature.IsEnabled = !feature.IsEnabled;
        await featureCatalogRepository.UpdateAsync(feature, ct);
        await unitOfWork.SaveChangesAsync(ct);

        TempData["Success"] = feature.IsEnabled
            ? "Фича включена."
            : "Фича выключена.";

        return RedirectToAction(nameof(Index));
    }
}