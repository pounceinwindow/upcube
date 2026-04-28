using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Web.Mapping;

namespace UpperCube.Web.Controllers;

public sealed class HomeController(IPropertyRepository repository) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = (await repository
                .GetLatestPublishedAsync(6))
                .Select(p => p.ToListItemDto());
        return View(model);
    }
}