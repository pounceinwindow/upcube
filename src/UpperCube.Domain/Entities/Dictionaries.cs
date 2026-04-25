using UpperCube.Domain.Common;

namespace UpperCube.Domain.Entities;

public sealed class City : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public ICollection<District> Districts { get; set; } = new List<District>();
}

public sealed class District : AuditableEntity
{
    public int CityId { get; set; }

    public City? City { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;
}

public sealed class PropertyType : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string IconClass { get; set; } = string.Empty;
}

public sealed class Category : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;
}

public sealed class Amenity : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string IconClass { get; set; } = string.Empty;
}
