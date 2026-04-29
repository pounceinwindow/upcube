using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Application.Abstractions.Valuation;
using UpperCube.Application.DTOs;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;

namespace UpperCube.Application.UseCases.Valuation;

public sealed class RegionalAverageValuator(IPropertyRepository propertyRepository) : IValuator
{
    private const string DefaultCurrency = "RUB";

    public string Name => "RegionalAverage";

    public async Task<ValuationResult> EstimateAsync(ValuationRequest request, CancellationToken ct = default)
    {
        var districtMatches = await SearchAsync(
            new PropertySearchFilter(
                request.CityId,
                request.DistrictId,
                request.PropertyTypeId,
                Status: (int)PropertyStatus.Published),
            ct);

        var matches = districtMatches.Count > 0
            ? districtMatches
            : await SearchAsync(
                new PropertySearchFilter(
                    request.CityId,
                    PropertyTypeId: request.PropertyTypeId,
                    Status: (int)PropertyStatus.Published),
                ct);

        if (matches.Count == 0) return new ValuationResult(0, 0, DefaultCurrency);

        var averagePricePerSquareMeter = matches
            .Where(x => x.Area.Value > 0)
            .Average(x => x.Price.Amount / x.Area.Value);

        var estimate = averagePricePerSquareMeter * request.Area;

        return new ValuationResult(
            decimal.Round(estimate * 0.85m, 2),
            decimal.Round(estimate * 1.15m, 2),
            DefaultCurrency);
    }

    private async Task<IReadOnlyList<Property>> SearchAsync(PropertySearchFilter filter, CancellationToken ct)
    {
        var (items, _) = await propertyRepository.SearchAsync(filter, 1, 100, ct);
        return items;
    }
}