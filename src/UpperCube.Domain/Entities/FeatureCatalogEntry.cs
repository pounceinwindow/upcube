using UpperCube.Domain.Common;

namespace UpperCube.Domain.Entities;

public sealed class FeatureCatalogEntry : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
}