using UpperCube.Application.DTOs;
using UpperCube.Domain.Entities;

namespace UpperCube.Web.Mapping;

public static class PropertyMapping
{
    public static PropertyListItemDto ToListItemDto(this Property property)
    {
        return new PropertyListItemDto(property.Id, property.Title, property.Price.Amount,
            property.Price.Currency, property.Area.Value, property.Rooms, property.City?.Name ?? string.Empty,
            property?.District?.Name ?? string.Empty,
            property?.Images.Where(x => x.IsPrimary)?.FirstOrDefault()?.ToString(), property!.Floor,
            property?.PropertyType?.Name ?? string.Empty, property!.TotalFloors, (int)property.TransactionType,
            property?.Address ?? string.Empty);
    }
}