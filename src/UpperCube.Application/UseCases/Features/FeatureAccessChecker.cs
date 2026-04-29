using System.Security.Claims;
using UpperCube.Application.Abstractions.Features;
using UpperCube.Application.Abstractions.Repositories;

namespace UpperCube.Application.UseCases.Features;

public sealed class FeatureAccessChecker(
    IFeatureCatalogRepository featureCatalogRepository,
    IEnumerable<IFeatureAccessPolicy> policies) : IFeatureAccessChecker
{
    public async Task<FeatureAccessResult> CheckAsync(string featureCode, ClaimsPrincipal user)
    {
        var feature = await featureCatalogRepository.GetByCodeAsync(featureCode);
        if (feature is null || !feature.IsEnabled) return new FeatureAccessResult(false, "Функция недоступна");

        foreach (var policy in policies)
        {
            var result = await policy.EvaluateAsync(featureCode, user);
            if (!result.IsAllowed) return result;
        }

        return new FeatureAccessResult(true);
    }
}