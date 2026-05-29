namespace UpperCube.Web.Areas.Admin.Models.Dictionaries;

public sealed class AdminDictionariesViewModel
{
    public IReadOnlyList<AdminDictionarySectionViewModel> Sections { get; set; } = [];
}