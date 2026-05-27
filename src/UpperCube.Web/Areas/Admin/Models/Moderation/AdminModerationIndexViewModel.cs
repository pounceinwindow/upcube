namespace UpperCube.Web.Areas.Admin.Models.Moderation;

public sealed class AdminModerationIndexViewModel
{
    public IReadOnlyList<AdminModerationItemViewModel> Items { get; set; } = [];
}
