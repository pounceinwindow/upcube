using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Application.Abstractions.Valuation;
using UpperCube.Application.DTOs;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;

namespace UpperCube.Application.UseCases.Valuation;

public sealed class ComparableSalesValuator(IPropertyRepository propertyRepository) : IValuator
{
    private const string DefaultCurrency = "RUB";

    public string Name => "ComparableSales";

    public async Task<ValuationResult> EstimateAsync(ValuationRequest request, CancellationToken ct = default)
    {
        var minArea = request.Area * 0.7m;
        var maxArea = request.Area * 1.3m;

        var (items, _) = await propertyRepository.SearchAsync(
            new PropertySearchFilter(
                request.CityId,
                PropertyTypeId: request.PropertyTypeId,
                Status: (int)PropertyStatus.Published,
                MinArea: minArea,
                MaxArea: maxArea),
            1,
            100,
            ct);

        var comparablePrices = items
            .Where(x => x.Area.Value > 0 && Math.Abs(x.Rooms - request.Rooms) <= 1)
            .OrderBy(x => SimilarityScore(x, request))
            .Take(5)
            .Select(x => x.Price.Amount / x.Area.Value)
            .Order()
            .ToArray();

        if (comparablePrices.Length == 0) return new ValuationResult(0, 0, DefaultCurrency);

        var median = Median(comparablePrices);
        var estimate = median * request.Area;

        return new ValuationResult(
            decimal.Round(estimate * 0.9m, 2),
            decimal.Round(estimate * 1.1m, 2),
            DefaultCurrency);
    }

    private static decimal Median(decimal[] values)
    {
        var middle = values.Length / 2;
        return values.Length % 2 == 1
            ? values[middle]
            : (values[middle - 1] + values[middle]) / 2;
    }

    private static decimal SimilarityScore(Property property, ValuationRequest request)
    {
        return Math.Abs(property.Area.Value - request.Area)
               + Math.Abs(property.Rooms - request.Rooms) * 10
               + Math.Abs(property.Floor - request.Floor) * 2
               + Math.Abs(property.TotalFloors - request.TotalFloors);
    }
}