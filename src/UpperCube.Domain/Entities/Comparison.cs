using UpperCube.Domain.Common;

namespace UpperCube.Domain.Entities;

public sealed class Comparison : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ICollection<ComparisonItem> Items { get; set; } = new List<ComparisonItem>();
}
