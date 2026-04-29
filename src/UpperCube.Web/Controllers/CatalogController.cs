using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Application.DTOs;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;
using UpperCube.Web.Mapping;
using UpperCube.Web.Models.Catalog;

namespace UpperCube.Web.Controllers;

public class CatalogController(IPropertyRepository repository, IDictionaryRepository<City> cityRepo , IDictionaryRepository<PropertyType> propertyTypeRepo) : Controller
{
    public async Task<IActionResult> Index(int? cityId,
        int? propertyTypeId,
        int? transactionType,
        decimal? minPrice,
        decimal? maxPrice,
        int? rooms,
        int page = 1)
    {
        var filter = new PropertySearchFilter(
            cityId,
            PropertyTypeId: propertyTypeId,
            TransactionType: transactionType,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            Rooms: rooms,
            Status: (int)PropertyStatus.Published);

        var city = (await cityRepo.ListAsync())
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });

        var property = (await propertyTypeRepo.ListAsync())
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });

        var (items, totalCount) = await repository.SearchAsync(filter, page, 9);
        var model = new CatalogModelView()
        {
            Cities = city, 
            PropertyTypes = property,
            CityId =  cityId,
            PropertyTypeId = propertyTypeId,
            TransactionType = transactionType,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Rooms = rooms,
            Items = items.Select(p => p.ToListItemDto()),
            TotalCount = totalCount,
            Page = page,
        };
        return View(model);
    }
}