namespace UpperCube.Web.Models.Compare;

public sealed class CompareModelView
{
    public const int MaxItems = 4;

    public bool IsAuthenticated { get; init; }

    public IReadOnlyList<CompareItemModelView> Items { get; init; } = [];

    public int Count => Items.Count;

    public bool HasItems => Items.Count > 0;
}

public sealed class CompareItemModelView
{
    public int Position { get; init; }

    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string Currency { get; init; } = string.Empty;

    public decimal Area { get; init; }

    public int Rooms { get; init; }

    public int Floor { get; init; }

    public int TotalFloors { get; init; }

    public string City { get; init; } = string.Empty;

    public string District { get; init; } = string.Empty;

    public string PropertyType { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string? PrimaryImagePath { get; init; }
}