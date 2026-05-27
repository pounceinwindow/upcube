namespace UpperCube.Web.Areas.Admin.Models.Moderation;

public sealed class AdminModerationItemViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string AgentId { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
