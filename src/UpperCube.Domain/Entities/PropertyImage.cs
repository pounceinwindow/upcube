using UpperCube.Domain.Common;
using UpperCube.Domain.Enums;

namespace UpperCube.Domain.Entities;

public sealed class PropertyImage : Entity
{
    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public string Path { get; set; } = string.Empty;

    public MediaType MediaType { get; set; } = MediaType.Photo;

    public bool IsPrimary { get; set; }

    public int Order { get; set; }

    public DateTime UploadedAt { get; set; }
}
