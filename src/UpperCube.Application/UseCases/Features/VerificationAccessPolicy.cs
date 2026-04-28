using System.Security.Claims;
using UpperCube.Application.Abstractions.Features;

namespace UpperCube.Application.UseCases.Features;

public sealed class VerificationAccessPolicy : IFeatureAccessPolicy
{
    public string PolicyName => "Verification";

    public Task<FeatureAccessResult> EvaluateAsync(string featureCode, ClaimsPrincipal user)
    {
        return Task.FromResult(new FeatureAccessResult(true));
    }
}