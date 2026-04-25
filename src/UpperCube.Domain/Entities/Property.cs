using UpperCube.Domain.Common;
using UpperCube.Domain.Enums;
using UpperCube.Domain.ValueObjects;

namespace UpperCube.Domain.Entities;

public sealed class Property : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Money Price { get; set; } = Money.Zero();

    public Area Area { get; set; } = new(0);

    public int Rooms { get; set; }

    public int Floor { get; set; }

    public int TotalFloors { get; set; }

    public TransactionType TransactionType { get; set; }

    public PropertyStatus Status { get; private set; } = PropertyStatus.Draft;

    public string AgentId { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public GeoPoint Location { get; set; } = new(null, null);

    public int CityId { get; set; }

    public City? City { get; set; }

    public int DistrictId { get; set; }

    public District? District { get; set; }

    public int PropertyTypeId { get; set; }

    public PropertyType? PropertyType { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public long ViewsCount { get; set; }

    public DateTime? PublishedAt { get; private set; }

    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();

    public ICollection<PropertyAmenity> Amenities { get; set; } = new List<PropertyAmenity>();

    public void Submit() => Status = PropertyStatus.PendingModeration;

    public void Approve(DateTime utcNow)
    {
        Status = PropertyStatus.Published;
        PublishedAt = utcNow;
    }

    public void Reject()
    {
        Status = PropertyStatus.Rejected;
        PublishedAt = null;
    }

    public void Archive()
    {
        Status = PropertyStatus.Archived;
        PublishedAt = null;
    }
}
