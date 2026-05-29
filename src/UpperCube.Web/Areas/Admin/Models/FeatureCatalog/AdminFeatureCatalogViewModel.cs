using UpperCube.Domain.Entities;

namespace UpperCube.Web.Areas.Admin.Models.FeatureCatalog;

public sealed class AdminFeatureCatalogViewModel
{
    public IReadOnlyList<FeatureCatalogEntry> Items { get; set; } = [];
}