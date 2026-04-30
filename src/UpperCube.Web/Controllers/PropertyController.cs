using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Web.Mapping;

namespace UpperCube.Web.Controllers;

public sealed class PropertyController(IPropertyRepository repository) : Controller
{
    public async Task<IActionResult> Details(int id)
    {
        var model = await repository
            .GetByIdAsync(id);

        if (model == null)
        {
            return NotFound();
        }
            
        await repository.IncrementViewsAsync(id);

        var details = model.ToDetailsDto(); 
        return View(details);
    }
}