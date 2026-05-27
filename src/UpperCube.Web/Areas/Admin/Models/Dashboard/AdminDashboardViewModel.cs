namespace UpperCube.Web.Areas.Admin.Models.Dashboard;

public sealed class AdminDashboardViewModel
{
    public int UsersCount { get; set; }

    public int PropertiesCount { get; set; }

    public int PendingPropertiesCount { get; set; }

    public int InquiriesCount { get; set; }

    public int ValuationsCount { get; set; }

    public int FeatureCatalogCount { get; set; }

    public int RecentErrorsCount { get; set; }

    public int RecentAuditLogsCount { get; set; }
}