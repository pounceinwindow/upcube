using Microsoft.AspNetCore.Mvc;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Entities;
using UpperCube.Web.Areas.Admin.Models.Dictionaries;

namespace UpperCube.Web.Areas.Admin.Controllers;

public sealed class DictionariesController(
    IDictionaryRepository<City> cityRepository,
    IDictionaryRepository<District> districtRepository,
    IDictionaryRepository<PropertyType> propertyTypeRepository,
    IDictionaryRepository<Category> categoryRepository,
    IDictionaryRepository<Amenity> amenityRepository) : AdminControllerBase
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cities = await cityRepository.ListAsync(ct);
        var districts = await districtRepository.ListAsync(ct);
        var propertyTypes = await propertyTypeRepository.ListAsync(ct);
        var categories = await categoryRepository.ListAsync(ct);
        var amenities = await amenityRepository.ListAsync(ct);

        var model = new AdminDictionariesViewModel
        {
            Sections =
            [
                new AdminDictionarySectionViewModel
                {
                    Title = "Города",
                    Items = cities
                        .OrderBy(x => x.Name)
                        .Select(x => new AdminDictionaryItemViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Slug = x.Slug
                        })
                        .ToList()
                },
                new AdminDictionarySectionViewModel
                {
                    Title = "Районы",
                    Items = districts
                        .OrderBy(x => x.Name)
                        .Select(x => new AdminDictionaryItemViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Slug = x.Slug,
                            Details = $"CityId: {x.CityId}"
                        })
                        .ToList()
                },
                new AdminDictionarySectionViewModel
                {
                    Title = "Типы недвижимости",
                    Items = propertyTypes
                        .OrderBy(x => x.Name)
                        .Select(x => new AdminDictionaryItemViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Slug = x.Slug,
                            Details = x.IconClass
                        })
                        .ToList()
                },
                new AdminDictionarySectionViewModel
                {
                    Title = "Категории",
                    Items = categories
                        .OrderBy(x => x.Name)
                        .Select(x => new AdminDictionaryItemViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Slug = x.Slug
                        })
                        .ToList()
                },
                new AdminDictionarySectionViewModel
                {
                    Title = "Удобства",
                    Items = amenities
                        .OrderBy(x => x.Name)
                        .Select(x => new AdminDictionaryItemViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Details = x.IconClass
                        })
                        .ToList()
                }
            ]
        };

        return View(model);
    }
}