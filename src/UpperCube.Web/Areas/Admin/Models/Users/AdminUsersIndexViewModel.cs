namespace UpperCube.Web.Areas.Admin.Models.Users;

public sealed class AdminUsersIndexViewModel
{
    public IReadOnlyList<AdminUserListItemViewModel> Users { get; set; } = [];
}