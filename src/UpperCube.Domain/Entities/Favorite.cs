using UpperCube.Domain.Common;

namespace UpperCube.Domain.Entities;

public sealed class Favorite : Entity
{
    public string UserId { get; set; } = string.Empty;

    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public DateTime AddedAt { get; set; }
}
