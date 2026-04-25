using UpperCube.Domain.Common;
using UpperCube.Domain.Enums;

namespace UpperCube.Domain.Entities;

public sealed class ModerationAction : AuditableEntity
{
    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public string ModeratorId { get; set; } = string.Empty;

    public ModerationActionType Action { get; set; }

    public string? Reason { get; set; }
}
