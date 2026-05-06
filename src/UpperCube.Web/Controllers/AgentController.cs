using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Domain.ValueObjects;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Mapping;
using UpperCube.Web.Models.Agent;

namespace UpperCube.Web.Controllers;

[Authorize(Roles = "Agent")]
[Route("agent")]
public sealed class AgentController(
    IPropertyRepository repository,
    UserManager<ApplicationUser> userManager,
    IDictionaryRepository<City> city,
    IDictionaryRepository<District> district,
    IDictionaryRepository<PropertyType> propertyType,
    IDictionaryRepository<Category> category) : Controller
{
    [HttpGet("properties")]
    public async Task<IActionResult> MyProperties(CancellationToken ct)
    {
        var userId = await userManager.GetUserAsync(HttpContext.User);
        if (userId == null) return Challenge();
        var property = await repository.GetByAgentIdAsync(userId.Id, ct);
        var propertyListItem = property
            .Select(x => x.ToListItemDto()).ToList();
        return View(propertyListItem);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var create = new PropertyFormModelView()
        {
            Title = "",
            Description = "",
            Price = 0,
            Currency = "RUB",
            DistrictId = 0,
            CityId = 0,
            Rooms = 1,
            Area = 0,
            Floor = 1,
            TotalFloors = 1,
            TransactionType = 1,
            PropertyTypeId = 0,
            CategoryId = 0,
            Address = ""
        };
        await PopulateDictionariesAsync(create, ct);
        return View(create);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("create")]
    public async Task<IActionResult> Create(PropertyFormModelView model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDictionariesAsync(model, ct);
            return View(model);
        }
        else
        {
            var agentId = userManager.GetUserId(HttpContext.User);
            if (agentId == null) return Challenge();
            var property = new Property()
            {
                Title = model.Title,
                Description = model.Description,
                Price = new Money(model.Price,model.Currency),
                Area = new Area(model.Area,AreaUnit.SquareMeter),
                Floor = model.Floor,
                TotalFloors = model.TotalFloors,
                Rooms = model.Rooms,
                PropertyTypeId = model.PropertyTypeId,
                CategoryId = model.CategoryId,
                Address = model.Address,
                TransactionType = (TransactionType)model.TransactionType,
                AgentId = agentId,
                DistrictId = model.DistrictId,
                CityId = model.CityId,
                
            };
            property.Submit();
            return RedirectToAction(nameof(MyProperties));
        }
    }

    private async Task PopulateDictionariesAsync(PropertyFormModelView model, CancellationToken ct)
    {
        var cities = await city.ListAsync(ct);
        var districts = await district.ListAsync(ct);
        var propertyTypes = await propertyType.ListAsync(ct);
        var categories = await category.ListAsync(ct);
        model.Cities = cities.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        model.Districts = districts.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        model.PropertyTypes = propertyTypes.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        model.Categories = categories.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
    }
}