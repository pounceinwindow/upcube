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

    public static PropertyDetailsDto ToDetailsDto(this Property property)
    {
        return new PropertyDetailsDto(property.Id, property.Title, property.Description, property.Price.Amount,
            property.Price.Currency, property.Area.Value, property.Rooms, property.Floor, property.TotalFloors,
            (int)property.Status, (int)property.TransactionType, property.City?.Name ?? string.Empty,
            property.District?.Name ?? string.Empty,
            property.PropertyType?.Name ?? string.Empty,
            property.Category?.Name ?? string.Empty, property.Address, property.AgentId, "Агент", property.ViewsCount,
            property.Images.Select(p => p.Path).ToList(), property.Amenities.Select(x => x.Amenity?.Name ?? "").ToList()
        );
    }
}