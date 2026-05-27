using UpperCube.Domain.Entities;

namespace UpperCube.Web.Areas.Admin.Models.Logs;

public sealed class AdminErrorLogsViewModel
{
    public IReadOnlyList<ErrorLogEntry> Items { get; set; } = [];

    public string? Warning { get; set; }
}