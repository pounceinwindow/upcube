using System.Security.Claims;

namespace UpperCube.Application.Abstractions.Features;

public interface IFeatureAccessPolicy
{
    string PolicyName { get; }

    Task<FeatureAccessResult> EvaluateAsync(string featureCode, ClaimsPrincipal user);
}

public interface IFeatureAccessChecker
{
    Task<FeatureAccessResult> CheckAsync(string featureCode, ClaimsPrincipal user);
}

public sealed record FeatureAccessResult(bool IsAllowed, string? DenyReason = null);
