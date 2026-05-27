using UpperCube.Domain.Entities;

namespace UpperCube.Web.Helpers;

public static class PropertyImagePathHelper
{
    public const string DefaultPropertyImagePath = "/homelengo/images/banner/banner-property-1.jpg";

    public static string? GetPrimaryOrFirstPath(IEnumerable<PropertyImage> images)
    {
        var orderedImages = images
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Id)
            .ToList();

        return orderedImages.FirstOrDefault(x => x.IsPrimary)?.Path
               ?? orderedImages.FirstOrDefault()?.Path;
    }

    public static string WithDefault(string? path)
    {
        return string.IsNullOrWhiteSpace(path) ? DefaultPropertyImagePath : path;
    }
}
