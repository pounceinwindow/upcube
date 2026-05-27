namespace UpperCube.Web.Areas.Admin.Models.Dictionaries;

public sealed class AdminDictionarySectionViewModel
{
    public string Title { get; set; } = string.Empty;

    public IReadOnlyList<AdminDictionaryItemViewModel> Items { get; set; } = [];
}
