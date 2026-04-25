using UpperCube.Domain.Common;

namespace UpperCube.Domain.Entities;

public sealed class Valuation : AuditableEntity
{
    public string? UserId { get; set; }

    public string InputJson { get; set; } = string.Empty;

    public decimal EstimatedMin { get; set; }

    public decimal EstimatedMax { get; set; }

    public string Currency { get; set; } = "USD";

    public string StrategyUsed { get; set; } = string.Empty;
}
