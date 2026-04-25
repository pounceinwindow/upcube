namespace UpperCube.Domain.Entities;

public sealed class PropertyAmenity
{
    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public int AmenityId { get; set; }

    public Amenity? Amenity { get; set; }
}
