namespace UpperCube.Application.Abstractions.Valuation;

public sealed record ValuationExplanationContext(
    string Culture,
    string? CityName,
    string? DistrictName,
    string? PropertyTypeName,
    decimal Area,
    int Rooms,
    int? Floor,
    int? TotalFloors,
    decimal EstimatedMin,
    decimal EstimatedMax,
    string Currency,
    string StrategyUsed,
    IReadOnlyList<ComparablePropertyContext> Comparables);

public sealed record ComparablePropertyContext(
    int PropertyId,
    string Title,
    string? DistrictName,
    decimal Area,
    decimal Price,
    decimal PricePerSquareMeter,
    int Rooms);
