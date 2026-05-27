using UpperCube.Domain.Entities;

namespace UpperCube.Web.Areas.Admin.Models.Logs;

public sealed class AdminAuditLogsViewModel
{
    public IReadOnlyList<AuditLogEntry> Items { get; set; } = [];

    public string? Warning { get; set; }
}