using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Application.DTOs;
using UpperCube.Domain.Enums;
using UpperCube.Web.Mapping;

namespace UpperCube.Web.Controllers;

public class CatalogController(IPropertyRepository repository) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = (await repository
            .SearchAsync(new PropertySearchFilter(Status:(int)PropertyStatus.Published), 1, 9))
            .Items
            .Select(p => p.ToListItemDto());
        
        return View(model);
    }
}
