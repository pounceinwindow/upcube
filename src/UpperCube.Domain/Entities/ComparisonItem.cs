namespace UpperCube.Domain.Entities;

public sealed class ComparisonItem
{
    public int ComparisonId { get; set; }

    public Comparison? Comparison { get; set; }

    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public int Position { get; set; }
}