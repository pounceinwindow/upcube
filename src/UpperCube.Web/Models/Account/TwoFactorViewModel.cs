using System.ComponentModel.DataAnnotations;

namespace UpperCube.Models.Account;

public sealed class TwoFactorViewModel
{
    [Required]
    [StringLength(16, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;

    public bool RememberMachine { get; set; }

    public bool RememberMe { get; set; }
}