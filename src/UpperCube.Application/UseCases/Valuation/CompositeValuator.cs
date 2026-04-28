using UpperCube.Application.Abstractions.Valuation;

namespace UpperCube.Application.UseCases.Valuation;

public sealed class CompositeValuator(
    RegionalAverageValuator regionalAverageValuator,
    ComparableSalesValuator comparableSalesValuator) : IValuator
{
    private const string DefaultCurrency = "RUB";

    private static readonly IReadOnlyDictionary<string, decimal> Weights = new Dictionary<string, decimal>
    {
        ["RegionalAverage"] = 0.4m,
        ["ComparableSales"] = 0.6m
    };

    public string Name => "Composite";

    public async Task<ValuationResult> EstimateAsync(ValuationRequest request, CancellationToken ct = default)
    {
        IValuator[] subValuators = [regionalAverageValuator, comparableSalesValuator];
        var results = new List<(string Name, ValuationResult Result)>();

        foreach (var valuator in subValuators)
        {
            var result = await valuator.EstimateAsync(request, ct);
            if (result.Min > 0 && result.Max > 0) results.Add((valuator.Name, result));
        }

        if (results.Count == 0) return new ValuationResult(0, 0, DefaultCurrency);

        var weightedSum = 0m;
        var weightSum = 0m;

        foreach (var (name, result) in results)
        {
            var weight = Weights.GetValueOrDefault(name, 1m);
            weightedSum += (result.Min + result.Max) / 2 * weight;
            weightSum += weight;
        }

        var weightedEstimate = weightedSum / weightSum;
        var min = results.Min(x => x.Result.Min);
        var max = results.Max(x => x.Result.Max);

        if (min <= 0 || max <= 0 || min > weightedEstimate || max < weightedEstimate)
        {
            min = Math.Min(min, weightedEstimate);
            max = Math.Max(max, weightedEstimate);
        }

        return new ValuationResult(decimal.Round(min, 2), decimal.Round(max, 2), DefaultCurrency);
    }
}