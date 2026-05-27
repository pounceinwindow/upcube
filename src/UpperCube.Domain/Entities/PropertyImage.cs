using UpperCube.Domain.Common;

namespace UpperCube.Domain.Entities;

public sealed class PropertyImage : Entity
{
    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public string Path { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int Order { get; set; }

    public DateTime UploadedAt { get; set; }
}