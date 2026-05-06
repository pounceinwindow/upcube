using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Application.DTOs;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Infrastructure.Identity;
using UpperCube.Web.Helpers;
using UpperCube.Web.Mapping;
using UpperCube.Web.Models.Agent;
using UpperCube.Web.Models.Catalog;

namespace UpperCube.Web.Controllers;

public class CatalogController(
    IPropertyRepository repository,
    IDictionaryRepository<City> cityRepo,
    IDictionaryRepository<PropertyType> propertyTypeRepo,
    IFavoriteRepository favoriteRepository,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(int? cityId,
        int? propertyTypeId,
        int? transactionType,
        decimal? minPrice,
        decimal? maxPrice,
        int? rooms,
        int page = 1,
        CancellationToken ct = default)
    {
        var filter = new PropertySearchFilter(
            cityId,
            PropertyTypeId: propertyTypeId,
            TransactionType: transactionType,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            Rooms: rooms,
            Status: (int)PropertyStatus.Published);

        var city = (await cityRepo.ListAsync(ct))
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });

        var property = (await propertyTypeRepo.ListAsync(ct))
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });

        var (items, totalCount) = await repository.SearchAsync(filter, page, 9, ct);
        var model = new CatalogModelView
        {
            Cities = city,
            PropertyTypes = property,
            CityId = cityId,
            PropertyTypeId = propertyTypeId,
            TransactionType = transactionType,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Rooms = rooms,
            Items = items.Select(p => p.ToListItemDto()),
            TotalCount = totalCount,
            Page = page
        };

        await FavoritesViewDataHelper.PopulateFavoriteIdsAsync(this, favoriteRepository, userManager, ct);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LoadMore(
        int? cityId,
        int? propertyTypeId,
        int? transactionType,
        decimal? minPrice,
        decimal? maxPrice,
        int? rooms,
        int page = 1,
        CancellationToken ct = default)
    {
        var filter = new PropertySearchFilter(
            cityId,
            PropertyTypeId: propertyTypeId,
            TransactionType: transactionType,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            Rooms: rooms,
            Status: (int)PropertyStatus.Published);

        var (items, _) = await repository.SearchAsync(filter, page, 9, ct);
        var dtos = items.Select(p => p.ToListItemDto()).ToList();

        await FavoritesViewDataHelper.PopulateFavoriteIdsAsync(this, favoriteRepository, userManager, ct);

        return PartialView("_PropertyCardList", dtos);
    }
}