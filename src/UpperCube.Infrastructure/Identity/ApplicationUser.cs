using Microsoft.AspNetCore.Identity;

namespace UpperCube.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? AvatarPath { get; set; }

    public string PreferredLanguage { get; set; } = "ru";

    public bool IsBlocked { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }
}
