namespace UpperCube.Web.Areas.Admin.Models.Users;

public sealed class AdminUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public IReadOnlyList<string> Roles { get; set; } = [];

    public bool EmailConfirmed { get; set; }

    public bool IsVerified { get; set; }

    public bool IsBlocked { get; set; }

    public DateTime CreatedAt { get; set; }
}
