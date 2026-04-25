namespace UpperCube.Application.Abstractions.Valuation;

public interface IValuator
{
    string Name { get; }

    Task<ValuationResult> EstimateAsync(ValuationRequest request, CancellationToken ct = default);
}

public sealed record ValuationRequest(
    int CityId,
    int DistrictId,
    int PropertyTypeId,
    decimal Area,
    int Rooms,
    int Floor,
    int TotalFloors);

public sealed record ValuationResult(decimal Min, decimal Max, string Currency);
