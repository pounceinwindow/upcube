namespace UpperCube.Domain.Entities;

public sealed class AuditLogEntry
{
    public string? Id { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime Timestamp { get; set; }
}