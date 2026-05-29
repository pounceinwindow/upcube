namespace UpperCube.Web.Areas.Admin.Models.Dictionaries;

public sealed class AdminDictionaryItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;
}